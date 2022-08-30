using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ByBit.Interfaces;
using ByBit.Models;
using Common;
using Common.Interfaces;
using Common.Models;
using Newtonsoft.Json;

namespace ByBit.Services
{
    public class HttpService : BaseHttpService, IHttpService
    {
        private readonly ISecretsService secretsService;
        private ExchangeApiKeysSecret apiKeys;
        private readonly HttpClient client;

        public HttpService(ISecretsService secretsService)
        {
            this.secretsService = secretsService;
            this.client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.bybit.com"),
            };

            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<TRes> GetAsync<TRes>(string path)
        {
            return this.GetAsync<TRes, object>(path, new object());
        }

        public async Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters)
        {
            var paramsString = await this.GetSignedRequestParams(parameters);

            var response = await client.GetAsync($"{path}?{paramsString}");
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        public async Task<TRes> DeleteAsync<TRes, TParams>(string path, TParams parameters)
        {
            var paramsString = await this.GetSignedRequestParams(parameters);

            var response = await client.DeleteAsync($"{path}?{paramsString}");
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        public async Task<BaseResponse> PostAsync<TBody>(string path, TBody body)
        {
            var paramsString = await this.GetSignedRequestParams(body);

            var response = await client.PostAsync($"{path}?{paramsString}", null);
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BaseResponse>(content);
        }

        private async Task<string> GetSignedRequestParams<T>(T data)
        {
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.ByBitApiKey);
            }

            var sortedDictionary = new SortedDictionary<string, object>();
            sortedDictionary.Add("api_key", this.apiKeys.apiKey);
            sortedDictionary.Add("timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds());

            var sortedParams = TypeDescriptor.GetProperties(data)
                .Cast<PropertyDescriptor>()
                .Select(pd => new { Name = pd.Name, Value = pd.GetValue(data) })
                .Aggregate(sortedDictionary, (acc, pair) =>
                {
                    acc.Add(pair.Name, pair.Value);
                    return acc;
                })
                .Select((pair) => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value.ToString())}");

            var paramsString = string.Join('&', sortedParams);

            var encoding = new ASCIIEncoding();

            byte[] payload = encoding.GetBytes(paramsString);
            byte[] secret = encoding.GetBytes(apiKeys.secretKey);

            byte[] hash;
            using (var hmac = new HMACSHA256(secret))
            {
                hash = hmac.ComputeHash(payload);
            }

            var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return $"{paramsString}&sign={sign}";
        }
    }
}
