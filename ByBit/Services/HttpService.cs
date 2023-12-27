using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
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
using System.Reflection.Metadata;

namespace ByBit.Services;

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

    public async Task<TRes> GetUnsignedAsync<TRes, TParams>(string path, TParams parameters)
    {
        var paramsString = this.GetRequestParamsString(parameters);

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

    public Task<TRes> GetAsync<TRes>(string path)
    {
        return this.SendV5Request<TRes>(path, HttpMethod.Get, null, null);
    }

    public Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters)
    {
        var paramsString = this.GetRequestParamsString(parameters);
        return this.SendV5Request<TRes>(path, HttpMethod.Get, paramsString, null);
    }

    public Task<TRes> PostAsync<TRes>(string path, string bodyString)
    {
        return this.SendV5Request<TRes>(path, HttpMethod.Post, null, bodyString);
    }

    //private async Task<string> GetSignedRequestParams<T>(T data)
    //{
    //    if (this.apiKeys == null)
    //    {
    //        this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.ByBitApiKey);
    //    }

    //    var sortedDictionary = new SortedDictionary<string, object>();
    //    sortedDictionary.Add("api_key", this.apiKeys.apiKey);
    //    sortedDictionary.Add("timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds());

    //    var sortedParams = TypeDescriptor.GetProperties(data)
    //        .Cast<PropertyDescriptor>()
    //        .Select(pd => new { Name = pd.Name, Value = pd.GetValue(data) })
    //        .Aggregate(sortedDictionary, (acc, pair) =>
    //        {
    //            acc.Add(pair.Name, pair.Value);
    //            return acc;
    //        })
    //        .Select((pair) => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value.ToString())}");

    //    var paramsString = string.Join('&', sortedParams);

    //    var encoding = new ASCIIEncoding();

    //    byte[] payload = encoding.GetBytes(paramsString);
    //    byte[] secret = encoding.GetBytes(apiKeys.secretKey);

    //    byte[] hash;
    //    using (var hmac = new HMACSHA256(secret))
    //    {
    //        hash = hmac.ComputeHash(payload);
    //    }

    //    var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

    //    return $"{paramsString}&sign={sign}";
    //}

    private async Task<TRes> SendV5Request<TRes>(string path, HttpMethod method, string paramsString, string serializedBody)
    {
        if (this.apiKeys == null)
        {
            this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.ByBitApiKey);
        }

        var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        var recvWindow = 5000;
        var bodyOrQueryParams = !string.IsNullOrEmpty(serializedBody) ? serializedBody : !string.IsNullOrEmpty(paramsString) ? paramsString : string.Empty;
        var payloadString = $"{timestamp}{this.apiKeys.apiKey}{recvWindow}{bodyOrQueryParams}";

        byte[] payload = Encoding.UTF8.GetBytes(payloadString);
        byte[] secret = Encoding.UTF8.GetBytes(this.apiKeys.secretKey);

        using var hmac = new HMACSHA256(secret);
        var hash = hmac.ComputeHash(payload);
        var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

        using var request = new HttpRequestMessage(method, string.IsNullOrEmpty(paramsString) ? path : $"{path}?{paramsString}");
        request.Headers.Add("X-BAPI-API-KEY", this.apiKeys.apiKey);
        request.Headers.Add("X-BAPI-TIMESTAMP", timestamp.ToString());
        request.Headers.Add("X-BAPI-SIGN", sign);
        request.Headers.Add("X-BAPI-RECV-WINDOW", recvWindow.ToString());

        if (!string.IsNullOrEmpty(serializedBody))
        {
            request.Content = new StringContent(serializedBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        using var response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            log.LogError($"request failed: {content}");
            throw new HttpRequestException($"\"{method}\" to \"{path}\" failed with code \"{response.StatusCode}\"");
        }

        log.LogInformation($"\"{method}\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

        return JsonConvert.DeserializeObject<TRes>(content);
    }


    private string GetRequestParamsString<T>(T data)
    {
        var sortedParams = TypeDescriptor.GetProperties(data)
            .Cast<PropertyDescriptor>()
            .Select(pd => new { Name = pd.Name, Value = pd.GetValue(data) })
            .Select((pair) => $"{pair.Name}={HttpUtility.UrlEncode(pair.Value.ToString())}");

        return string.Join('&', sortedParams);
    }
}

