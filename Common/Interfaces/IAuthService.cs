using System;
using Common.Models;
using Microsoft.AspNetCore.Http;

namespace Common.Interfaces
{
    public interface IAuthService
    {
        AzureUser GetAzureUser(HttpRequest req);
        string GetUserId(HttpRequest req);
        string GetUserId(HttpRequestMessage req);
        void ValidateUser(HttpRequest req);
    }
}

