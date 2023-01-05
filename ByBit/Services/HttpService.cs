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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ByBit.Services
{
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

        public Task<TRes> GetAsync<TRes>(string path)
        {
            return this.GetAsync<TRes, object>(path, new object());
        }

        public async Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters)
        {
            var paramsString = await this.GetSignedRequestParams(parameters);

            var response = await client.GetAsync($"{path}?{paramsString}");
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"request failed: {content}");
                throw new HttpRequestException($"\"GET\" to \"{path}\" failed with code \"{response.StatusCode}\"");
            }

            log.LogInformation($"\"GET\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        public async Task<TRes> DeleteAsync<TRes, TParams>(string path, TParams parameters)
        {
            var paramsString = await this.GetSignedRequestParams(parameters);

            var response = await client.DeleteAsync($"{path}?{paramsString}");
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"request failed: {content}");
                throw new HttpRequestException($"\"DELETE\" to \"{path}\" failed with code \"{response.StatusCode}\"");
            }

            log.LogInformation($"\"DELETE\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        public async Task<BaseResponse> PostAsync<TBody>(string path, TBody body)
        {
            var paramsString = await this.GetSignedRequestParams(body);

            var response = await client.PostAsync($"{path}?{paramsString}", null);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"request failed: {content}");
                throw new HttpRequestException($"\"POST\" to \"{path}\" failed with code \"{response.StatusCode}\"");
            }

            log.LogInformation($"\"POST\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

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
