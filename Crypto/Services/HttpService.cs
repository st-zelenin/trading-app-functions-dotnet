using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Common.Models;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Crypto.Services
{
    internal class RequestBody<T>
    {
        public long id { get; private set; }
        public string method { get; private set; }
        public string api_key { get; private set; }

        [JsonProperty("params")]
        public T body { get; private set; }

        public long nonce { get; private set; }
        public string sig { get; private set; }

        public RequestBody(T body, string method)
        {
            this.body = body;
            this.nonce = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            this.id = this.nonce;
            this.method = method;
        }

        public RequestBody<T> Sign(ExchangeApiKeysSecret apiKeys)
        {
            this.api_key = apiKeys.apiKey;

            var paramsString = TypeDescriptor.GetProperties(this.body)
                .Cast<PropertyDescriptor>()
                .Select(pd => new { Name = pd.Name, Value = pd.GetValue(this.body) })
                .OrderBy(pair => pair.Name)
                .Aggregate("", (acc, pair) => acc + pair.Name + pair.Value.ToString());

            var sigPayload = this.method + this.id + this.api_key + paramsString + this.nonce;

            var encoding = new ASCIIEncoding();

            byte[] payload = encoding.GetBytes(sigPayload);
            byte[] secret = encoding.GetBytes(apiKeys.secretKey);

            byte[] hash;
            using (var hmac = new HMACSHA256(secret))
            {
                hash = hmac.ComputeHash(payload);
            }

            this.sig = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return this;
        }
    }

    public class HttpService : BaseHttpService, IHttpService
    {
        private readonly ISecretsService secretsService;
        private readonly ILogger<HttpService> log;
        private readonly HttpClient client;
        private ExchangeApiKeysSecret apiKeys;

        public HttpService(ISecretsService secretsService, ILogger<HttpService> log, HttpClient client)
        {
            this.secretsService = secretsService;
            this.log = log;
            this.client = client;
        }

        public Task<TRes> PostAsync<TRes>(string path)
        {
            return this.PostAsync<TRes, object>(path, new object());
        }

        public Task<BaseResponse> PostAsync<TBody>(string path, TBody body)
        {
            return this.PostAsync<BaseResponse, TBody>(path, body);
        }

        public async Task<TRes> PostAsync<TRes, TBody>(string path, TBody body)
        {
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.CryptoApiKey);
            }

            var signedBody = new RequestBody<TBody>(body, path).Sign(this.apiKeys);
            string jsonString = JsonConvert.SerializeObject(signedBody);


            var response = await client.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, MediaTypeNames.Application.Json));
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            if (content.Equals("Too Many Requests"))
            {
                throw new TooManyRequestsException();
            }

            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"request failed: {content}");
                throw new HttpRequestException($"\"POST\" to \"{path}\" failed with code \"{response.StatusCode}\"");
            }

            log.LogInformation($"\"POST\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        public async Task<TRes> GetAsync<TRes>(string path)
        {
            var response = await this.client.GetAsync(path);
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TRes>(content);
        }
    }
}

