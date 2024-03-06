using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common;
using Common.Interfaces;
using Common.Models;
using Gate.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gate.Services
{
    public class HttpService : BaseHttpService, IHttpService
    {
        private readonly ISecretsService secretsService;
        private readonly ILogger<HttpService> log;
        private readonly HttpClient client;
        private ExchangeApiKeysSecret apiKeys;

        const string PREFIX = "/api/v4";

        public HttpService(ISecretsService secretsService, ILogger<HttpService> log, HttpClient client)
        {
            this.secretsService = secretsService;
            this.log = log;
            this.client = client;
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
            return this.GetSignatureAsync<TRes, TBody, object>(HttpMethod.Post, path, body, null);
        }

        public Task<TRes> PostAsync<TRes, TBody, TQuery>(string path, TBody body, TQuery query)
        {
            return this.GetSignatureAsync<TRes, TBody, TQuery>(HttpMethod.Post, path, body, query);
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

            var serializedBody = body == null ? string.Empty : JsonConvert.SerializeObject(body);
            var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var queryString = this.GetQueryString(query);
            var signature = this.GetSignature(method, path, timestamp, serializedBody, queryString);
            var url = $"https://api.gateio.ws{PREFIX}{path}";

            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}"),
                Method = method,
            };

            if (body != null)
            {
                requestMessage.Content = new StringContent(serializedBody, Encoding.UTF8, MediaTypeNames.Application.Json);
            }

            requestMessage.Headers.Add("KEY", this.apiKeys.apiKey);
            requestMessage.Headers.Add("Timestamp", timestamp.ToString());
            requestMessage.Headers.Add("SIGN", signature);

            var response = await this.client.SendAsync(requestMessage);

            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"request failed: {content}");
                throw new HttpRequestException($"{method} to {path} failed with code {response.StatusCode}, reason: {content}");
            }

            log.LogInformation($"\"{method}\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

            return JsonConvert.DeserializeObject<TRes>(content);
        }

        private string GetQueryString<T>(T query)
        {
            if (query == null)
            {
                return "";
            }

            var paramsPairs = TypeDescriptor.GetProperties(query)
                .Cast<PropertyDescriptor>()
                .Aggregate(new List<string>(), (acc, curr) =>
                {
                    acc.Add($"{curr.Name}={HttpUtility.UrlEncode(curr.GetValue(query).ToString())}");
                    return acc;
                });

            return string.Join('&', paramsPairs);
        }

        private string GetSignature(HttpMethod method, string path, long timestamp, string body, string query)
        {
            var encoding = new ASCIIEncoding();
            var bodyHashString = this.GetBodyHash(body, encoding);

            var formattedMessage = $"{method}\n{PREFIX}{path}\n{query}\n{bodyHashString}\n{timestamp}";

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

            return BitConverter.ToString(bodyHash).Replace("-", "").ToLower();
        }
    }
}

