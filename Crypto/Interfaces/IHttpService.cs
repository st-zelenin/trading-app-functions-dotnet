using System;
using System.Threading.Tasks;

namespace Crypto.Interfaces
{
    public interface IHttpService
    {
        Task<TRes> GetAsync<TRes>(string path);
        Task<TRes> PostAsync<TRes>(string path);
        Task<TRes> PostAsync<TRes, TBody>(string path, TBody body) where TBody : new();
    }
}

