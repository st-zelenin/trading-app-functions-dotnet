using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Common.Models;
using Crypto.Interfaces;
using Crypto.Models;
using Microsoft.AspNetCore.Http;
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

    public class HttpService : IHttpService
    {
        private readonly ISecretsService secretsService;
        private ExchangeApiKeysSecret apiKeys;
        private readonly HttpClient client;

        public HttpService(ISecretsService secretsService)
        {
            this.secretsService = secretsService;
            this.client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.crypto.com/v2/"),
            };

            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<T> GetRequestBody<T>(HttpRequest req)
        {
            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            return JsonConvert.DeserializeObject<T>(requestBody);
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

            Console.WriteLine(jsonString);

            var response = await client.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

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

