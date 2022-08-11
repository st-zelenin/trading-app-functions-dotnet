using System;
using Microsoft.AspNetCore.Http;

namespace Common.Interfaces
{
    public interface IBaseHttpService
    {
        Task<T> GetRequestBody<T>(HttpRequest req);
        string GetRequiredQueryParam(HttpRequest req, string key);
    }
}

