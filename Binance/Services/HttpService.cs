using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Binance.Interfaces;
using Common;
using Common.Interfaces;
using Common.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Binance.Services;

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
        return this.GetAsync<TRes, object>(path, null, false);
    }

    public Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters)
    {
        return this.GetAsync<TRes, TParams>(path, parameters, false);
    }

    public Task<TRes> GetSignedAsync<TRes>(string path)
    {
        return this.GetAsync<TRes, object>(path, null, true);
    }

    public Task<TRes> GetSignedAsync<TRes, TParams>(string path, TParams parameters)
    {
        return this.GetAsync<TRes, TParams>(path, parameters, true);
    }

    public Task<TRes> PostSignedAsync<TRes>(string path)
    {
        return this.PostSignedAsync<TRes, object>(path, null);
    }

    public async Task<TRes> PostSignedAsync<TRes, TParams>(string path, TParams parameters)
    {
        var queryString = await this.GetSignedQueryString(parameters);

        log.LogDebug($"{path}?{queryString}");

        var response = await client.PostAsync($"{path}?{queryString}", null);
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            log.LogError($"request failed: {content}");
            throw new HttpRequestException($"\"POST\" to \"{path}\" failed: \"{content}\"");
        }

        log.LogInformation($"\"POST\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

        return JsonConvert.DeserializeObject<TRes>(content);
    }


    public async Task<TRes> DeleteSignedAsync<TRes, TParams>(string path, TParams parameters)
    {
        var queryString = await this.GetSignedQueryString(parameters);

        log.LogDebug($"{path}?{queryString}");

        var response = await client.DeleteAsync($"{path}?{queryString}");
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            log.LogError($"request failed: {content}");
            throw new HttpRequestException($"\"DELETE\" to \"{path}\" failed: \"{content}\"");
        }

        log.LogInformation($"\"DELETE\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

        return JsonConvert.DeserializeObject<TRes>(content);
    }

    private async Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters, bool signed)
    {
        var queryString = signed ? await this.GetSignedQueryString(parameters) : this.GetQueryString(parameters);

        log.LogDebug($"{path}?{queryString}");

        var response = await client.GetAsync(queryString.Length > 0 ? $"{path}?{queryString}" : path);
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            log.LogError($"request failed: {content}");
            throw new HttpRequestException($"\"GET\" to \"{path}\" failed: \"{content}\"");
        }

        log.LogInformation($"\"GET\" to \"{path}\" succeeded with code \"{response.StatusCode}\"");

        return JsonConvert.DeserializeObject<TRes>(content);
    }

    private async Task<StringBuilder> GetSignedQueryString<T>(T data)
    {
        if (this.apiKeys == null)
        {
            this.apiKeys = await this.secretsService.GetSecretAsync<ExchangeApiKeysSecret>(SecretsKeys.BinanceApiKey);
            if (!this.client.DefaultRequestHeaders.Contains("X-MBX-APIKEY"))
            {
                this.client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.apiKeys.apiKey);
            }
        }

        var paramsBuilder = this.GetQueryString(data);
        var prefix = paramsBuilder.Length > 0 ? "&" : string.Empty;
        paramsBuilder.Append($"{prefix}timestamp={new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}");

        byte[] payload = Encoding.UTF8.GetBytes(paramsBuilder.ToString());
        byte[] secret = Encoding.UTF8.GetBytes(apiKeys.secretKey);

        byte[] hash;
        using (var hmac = new HMACSHA256(secret))
        {
            hash = hmac.ComputeHash(payload);
        }

        var sign = BitConverter.ToString(hash).Replace("-", "").ToLower();
        paramsBuilder.Append($"&signature={sign}");

        return paramsBuilder;
    }

    private StringBuilder GetQueryString<T>(T data)
    {
        var builder = new StringBuilder();

        if (data == null)
        {
            return builder;
        }

        if (data != null)
        {
            _ = TypeDescriptor.GetProperties(data)
                .Cast<PropertyDescriptor>()
                .Aggregate(builder, (acc, curr) =>
                {
                    if (typeof(IEnumerable<string>).IsAssignableFrom(curr.PropertyType))
                    {
                        this.buildQueryStringArrayParam(builder, curr.GetValue(data) as IEnumerable<string>, curr.Name);
                    }
                    else
                    {
                        acc.AppendFormat("{0}={1}&", curr.Name, curr.GetValue(data));
                        //acc.AppendFormat("{0}={1}&", curr.Name, HttpUtility.UrlEncode(curr.GetValue(data).ToString()));
                        //acc.Append($"{curr.Name}={HttpUtility.UrlEncode(curr.GetValue(data).ToString())}&");
                    }

                    return acc;
                });
        }

        builder.Remove(builder.Length - 1, 1);

        return builder;
    }

    private void buildQueryStringArrayParam(StringBuilder builder, IEnumerable<string> array, string paramName)
    {
        if (!array.Any())
        {
            return;
        }

        builder.AppendFormat("{0}=[", paramName);
        _ = array.Aggregate(builder, (acc, curr) =>
        {
            acc.AppendFormat("\"{0}\",", curr);

            return acc;
        });
        builder.Remove(builder.Length - 1, 1);
        builder.Append("]&");
    }
}

