using System;
using Common.Interfaces;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;

namespace Common
{
    public class AuthService: IAuthService
    {
        public string GetUserId(HttpRequest req)
        {
            string authorizationHeader = req.Headers["Authorization"];

            return this.DecodeAuthorizationHeader(authorizationHeader);
        }

        public string GetUserId(HttpRequestMessage req)
        {
            var authorizationHeader = req.Headers.GetValues("Authorization").First();

            return this.DecodeAuthorizationHeader(authorizationHeader);
        }

        public void ValidateUser(HttpRequest req)
        {
            this.GetUserId(req);
        }

        private string DecodeAuthorizationHeader(string authorizationHeader)
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

            var dictionary = JwtBuilder.Create()
                .WithValidationParameters(validationParameters)
                .WithAlgorithm(new HMACSHA256Algorithm())
                .Decode<IDictionary<string, string>>(token);

            string? oid;
            dictionary.TryGetValue("oid", out oid);

            if (string.IsNullOrEmpty(oid))
            {
                throw new Exception("oid missing");
            }

            return oid;
        }
    }
}

