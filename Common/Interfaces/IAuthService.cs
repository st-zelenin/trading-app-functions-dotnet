using System;
using Microsoft.AspNetCore.Http;

namespace Common.Interfaces
{
    public interface IAuthService
    {
        string GetUserId(HttpRequest req);
        string GetUserId(HttpRequestMessage req);
        void ValidateUser(HttpRequest req);
    }
}

