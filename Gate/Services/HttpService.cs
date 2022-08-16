using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Gate.Interfaces;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Common;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace Gate.Services
{
    public class HttpService : BaseHttpService, IHttpService
    {
        private readonly ISecretsService secretsService;
        private ExchangeApiKeysSecret apiKeys;
        private readonly HttpClient client;

        const string PREFIX = "/api/v4";

        public HttpService(ISecretsService secretsService)
        {
            this.secretsService = secretsService;
            this.client = new HttpClient()
            {
                //BaseAddress = new Uri("https://api.gateio.ws/api/v4"),
            };

            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<TRes> GetAsync<TRes>(string path)
        {
            return this.GetSignatureAsync<TRes, object, object>(HttpMethod.Get, path, null, null);
        }

        public Task<TRes> GetAsync<TRes, TQuery>(string path, TQuery query)
        {
            return this.GetSignatureAsync<TRes, object, TQuery>(HttpMethod.Get, path, null, query);
        }

        public Task<TRes> PostAsync<TRes, TBody>(string path, TBody body)
        {
            return this.GetSignatureAsync<TRes, TBody, object>(HttpMethod.Get, path, body, null);
        }

        public Task<TRes> PostAsync<TRes, TBody, TQuery>(string path, TBody body, TQuery query)
        {
            return this.GetSignatureAsync<TRes, TBody, TQuery>(HttpMethod.Get, path, body, query);
        }

        public Task<TRes> DeleteAsync<TRes, TQuery>(string path, TQuery query)
        {
            return this.GetSignatureAsync<TRes, object, TQuery>(HttpMethod.Delete, path, null, query);
        }

        private async Task<TRes> GetSignatureAsync<TRes, TBody, TQuery>(HttpMethod method, string path, TBody body, TQuery query)
        {
            if (this.apiKeys == null)
            {
                this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.GateApiKey);
                this.client.DefaultRequestHeaders.Add("KEY", this.apiKeys.apiKey);
            }

            var serializedBody = body == null ? "" : JsonConvert.SerializeObject(body);

            //var encoding = new ASCIIEncoding();
            //byte[] payload = encoding.GetBytes(serializedBody);

            //byte[] bodyHash;
            //using (var hmac = new HMACSHA512())
            //{
            //    bodyHash = hmac.ComputeHash(payload);
            //}

            //var hashedPayload = BitConverter.ToString(bodyHash).Replace("-", "").ToLower(); // TODO: ????
            var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); // TODO: ????
            //var formattedMessage = $"{method}\n{PREFIX}{path}\n{query}\n{hashedPayload}\n{timestamp}";

            //Console.WriteLine($"formattedMessage = {formattedMessage}");

            //byte[] secret = encoding.GetBytes(this.apiKeys.secretKey);
            //byte[] message = encoding.GetBytes(formattedMessage);

            //byte[] headerHash;
            //using (var hmac = new HMACSHA512(secret))
            //{
            //    headerHash = hmac.ComputeHash(message);
            //}


            var queryString = this.GetQueryString(query);
            //var queryString = "";
            //if (query != null)
            //{
            //    queryString = TypeDescriptor.GetProperties(query)
            //        .Cast<PropertyDescriptor>()
            //        .Aggregate("", (acc, curr) =>
            //        {
            //            acc += $"{curr.Name}={HttpUtility.UrlEncode(curr.GetValue(query).ToString())}";
            //            return acc;
            //        });
            //}

            var signature = this.GetSignature(method, path, timestamp, serializedBody, queryString);

            Console.WriteLine($"signature = {signature}");



            //var queryDictionary = new Dictionary<string, string>();
            //if (query != null)
            //{
            //    TypeDescriptor.GetProperties(query)
            //        .Cast<PropertyDescriptor>()
            //        .Aggregate(queryDictionary, (acc, curr) =>
            //        {
            //            acc.Add(curr.Name, HttpUtility.UrlEncode(curr.GetValue(query).ToString()));
            //            return acc;
            //        });
            //}

            //var uriString = QueryHelpers.AddQueryString($"https://api.gateio.ws/api/v4{PREFIX}/{path}", queryDictionary);

            var url = $"https://api.gateio.ws{PREFIX}{path}";

            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}"),
                Method = method,
            };

            if (body != null)
            {
                var jsonString = JsonConvert.SerializeObject(body);
                requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            requestMessage.Headers.Add("KEY", this.apiKeys.apiKey);
            requestMessage.Headers.Add("Timestamp", timestamp.ToString());
            requestMessage.Headers.Add("SIGN", signature);

            var response = await this.client.SendAsync(requestMessage);

            //response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"content: {content}");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        private string GetQueryString<T>(T query)
        {
            return query == null ? ""
                : TypeDescriptor.GetProperties(query)
                    .Cast<PropertyDescriptor>()
                    .Aggregate("", (acc, curr) =>
                    {
                        acc += $"{curr.Name}={HttpUtility.UrlEncode(curr.GetValue(query).ToString())}";
                        return acc;
                    });
        }

        private string GetSignature(HttpMethod method, string path, long timestamp, string body, string query)
        {
            var encoding = new ASCIIEncoding();
            //byte[] bodyBytes = encoding.GetBytes(body);

            //byte[] bodyHash;
            //using (var hmac = new HMACSHA512())
            //{
            //    bodyHash = hmac.ComputeHash(bodyBytes);
            //}

            var bodyHashString = this.GetBodyHash(body, encoding);
            //var bodyHashString = BitConverter.ToString(bodyHash).Replace("-", "").ToLower(); // TODO: ????
            var formattedMessage = $"{method}\n{PREFIX}{path}\n{query}\n{bodyHashString}\n{timestamp}";

            Console.WriteLine($"formattedMessage = {formattedMessage}");

            byte[] secret = encoding.GetBytes(this.apiKeys.secretKey);
            byte[] message = encoding.GetBytes(formattedMessage);

            byte[] headerHash;
            using (var hmac = new HMACSHA512(secret))
            {
                headerHash = hmac.ComputeHash(message);
            }

            return BitConverter.ToString(headerHash).Replace("-", "").ToLower();
        }

        private string GetBodyHash(string body, ASCIIEncoding encoding)
        {
            byte[] bodyBytes = encoding.GetBytes(body);
            byte[] bodyHash;
            using (var sha512 = SHA512.Create())
            {
                bodyHash = sha512.ComputeHash(bodyBytes);
            }

            return BitConverter.ToString(bodyHash).Replace("-", "").ToLower(); // TODO: ????
        }
    }
}

