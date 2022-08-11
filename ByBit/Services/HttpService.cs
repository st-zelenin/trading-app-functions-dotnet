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
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.ByBitApiKey);
            }

            var sortedDictionary = new SortedDictionary<string, object>();
            sortedDictionary.Add("api_key", this.apiKeys.apiKey);
            sortedDictionary.Add("timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds());

            var sortedParams = TypeDescriptor.GetProperties(parameters)
                .Cast<PropertyDescriptor>()
                .Select(pd => new { Name = pd.Name, Value = pd.GetValue(parameters) })
                .Aggregate(sortedDictionary, (acc, pair) =>
                {
                    acc.Add(pair.Name, pair.Value);
                    return acc;
                })
                .Select((pair) => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value.ToString())}");

            var paramsString = string.Join('&', sortedParams);

            Console.WriteLine(paramsString);

            var encoding = new ASCIIEncoding();

            byte[] payload = encoding.GetBytes(paramsString);
            byte[] secret = encoding.GetBytes(apiKeys.secretKey);

            byte[] hash;
            using (var hmac = new HMACSHA256(secret))
            {
                hash = hmac.ComputeHash(payload);
            }

            var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var response = await client.GetAsync($"{path}?{paramsString}&sign={sign}");
            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            //Console.WriteLine($"content: {content}");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        //public async Task<TRes> PostAsync<TRes, TBody>(string path, TBody body)
        //{
        //    if (this.apiKeys == null)
        //    {
        //        this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.ByBitApiKey);
        //    }

        //    var sortedDictionary = TypeDescriptor.GetProperties(body)
        //        .Cast<PropertyDescriptor>()
        //        .Select(pd => new { Name = pd.Name, Value = pd.GetValue(body) })
        //        .Aggregate(new SortedDictionary<string, object>(), (acc, pair) => {
        //            acc.Add(pair.Name, pair.Value);
        //            return acc;
        //        });

        //    sortedDictionary.Add("api_key", this.apiKeys.apiKey);
        //    sortedDictionary.Add("timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds());

        //    var jsonString = JsonConvert.SerializeObject(sortedDictionary);

        //    Console.WriteLine(jsonString);

        //    var encoding = new ASCIIEncoding();

        //    byte[] payload = encoding.GetBytes(jsonString);
        //    byte[] secret = encoding.GetBytes(apiKeys.secretKey);

        //    byte[] hash;
        //    using (var hmac = new HMACSHA256(secret))
        //    {
        //        hash = hmac.ComputeHash(payload);
        //    }

        //    var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

        //    sortedDictionary.Add("sign", sign);


        //    var response = await client.PostAsync(path,
        //        new StringContent(JsonConvert.SerializeObject(sortedDictionary), Encoding.UTF8, "application/json"));
        //    //response.EnsureSuccessStatusCode();
        //    string content = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine($"content: {content}");

        //    return JsonConvert.DeserializeObject<TRes>(content);
        //}
    }
}
