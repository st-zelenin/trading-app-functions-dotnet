using System;
using Common.Interfaces;
using JWT;
using JWT.Algorithms;
using JWT.Builder;

namespace Common
{
    public class AuthService: IAuthService
    {
        public string GetUserId(string authorizationHeader)
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

        public void ValidateUser(string authorizationHeader)
        {
            this.GetUserId(authorizationHeader);
        }
    }
}

