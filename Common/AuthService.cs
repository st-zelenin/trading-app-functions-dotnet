using System;
using JWT;
using JWT.Algorithms;
using JWT.Builder;

namespace Common
{
    public class AuthService
    {
        public static string GetUserId(string authorizationHeader )
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                throw new Exception("authorizationHeader missing");
            }

            string token = authorizationHeader.Substring(7);

            ValidationParameters validationParameters = ValidationParameters.Default;
            validationParameters.ValidateSignature = false;

            var json = JwtBuilder.Create()
                .WithValidationParameters(validationParameters)
                .WithAlgorithm(new HMACSHA256Algorithm())
                .Decode<IDictionary<string, string>>(token);

            string userId;

            bool v = json.TryGetValue("oid", out userId);

            if (!v)
            {
                throw new Exception("oid missing");
            }

            return userId;
        }
    }
}

