using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Common.Models;
using Crypto.Interfaces;
using Newtonsoft.Json;

namespace Crypto.Services
{
    internal class RequestBody<T> where T : new()
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
            this.body = body == null ? (T)new object() : body;
            this.nonce = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            this.id = this.nonce;
            this.method = method;
        }

        public RequestBody<T> Sign(ExchangeApiKeysSecret apiKeys)
        {
            this.api_key = apiKeys.apiKey;

            var paramsString = typeof(T)
                .GetProperties()
                .Select(pi => new { Name = pi.Name, Value = pi.GetValue(this.body) })
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
        private readonly MediaTypeFormatter formatter;

        public HttpService(ISecretsService secretsService)
        {
            this.secretsService = secretsService;
            this.client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.crypto.com/v2/"),
            };

            this.client.DefaultRequestHeaders.Add("Accept", "application/json");

            this.formatter = new JsonMediaTypeFormatter();
        }

        private async Task SignRequestBody(string method, NameValueCollection body)
        {
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecret<ExchangeApiKeysSecret>(SecretsKeys.CryptoApiKey);
            }

            var nonce = DateTime.Now.Ticks;
            var id = nonce;

            var paramsString = body.AllKeys
                .OrderBy(a => a)
                .Aggregate("", (acc, key) => acc + key + body.Get(key));

            var sigPayload = method + id + apiKeys.apiKey + paramsString + nonce;

            ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] payload = encoding.GetBytes(sigPayload);
            byte[] secret = encoding.GetBytes(apiKeys.secretKey);
            byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(secret))
            {
                hashBytes = hash.ComputeHash(payload);
            }
        }

        public async Task<TRes> Post<TRes, TBody>(string path, TBody body) where TBody : new()
        {
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecret<ExchangeApiKeysSecret>(SecretsKeys.CryptoApiKey);
            }

            var signedBody = new RequestBody<TBody>(body, path).Sign(this.apiKeys);
            string jsonString = JsonConvert.SerializeObject(signedBody);

            Console.WriteLine(jsonString);

            var response = await client.PostAsync(path, new StringContent(jsonString, Encoding.UTF8, "application/json"));
            //response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TRes>(responseBody);
        }
    }
}

