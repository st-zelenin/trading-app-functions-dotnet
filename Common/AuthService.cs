﻿using Common.Interfaces;
using Common.Models;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;

namespace Common;

public class AuthService : IAuthService
{
    public AzureUser GetAzureUser(HttpRequest req)
    {
        string authorizationHeader = req.Headers["Authorization"];

        var dictionary = DecodeAuthorizationHeader(authorizationHeader);

        var oid = GetDecodedValue(dictionary, "oid");
        var name = GetDecodedValue(dictionary, "name");

        return new AzureUser()
        {
            oid = oid,
            name = name
        };
    }

    public string GetUserId(HttpRequest req)
    {
        string authorizationHeader = req.Headers["Authorization"];

        //return this.GetOid(authorizationHeader) + "000111222333";
        return this.GetOid(authorizationHeader);
    }

    public string GetUserId(HttpRequestMessage req)
    {
        var authorizationHeader = req.Headers.GetValues("Authorization").First();

        return this.GetOid(authorizationHeader);
    }

    public void ValidateUser(HttpRequest req)
    {
        this.GetUserId(req);
    }

    private static IDictionary<string, string> DecodeAuthorizationHeader(string authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            throw new ArgumentException("authorizationHeader missing");
        }

        const string TOKEN_START = "Bearer ";

        if (!authorizationHeader.StartsWith(TOKEN_START))
        {
            throw new ArgumentException("bearer token is expected");
        }

        var token = authorizationHeader.Substring(TOKEN_START.Length);

        var validationParameters = ValidationParameters.Default;
        validationParameters.ValidateSignature = false;

        return JwtBuilder.Create()
            .WithValidationParameters(validationParameters)
            .WithAlgorithm(new HMACSHA256Algorithm())
            .Decode<IDictionary<string, string>>(token);
    }

    private string GetOid(string authorizationHeader)
    {
        var dictionary = DecodeAuthorizationHeader(authorizationHeader);
        return GetDecodedValue(dictionary, "oid");
    }

    private static string GetDecodedValue(IDictionary<string, string> dictionary, string key)
    {
        dictionary.TryGetValue(key, out string? value);

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"key '{key}' is missing");
        }

        return value;
    }
}

