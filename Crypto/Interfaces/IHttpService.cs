using System;
using System.Threading.Tasks;
using Crypto.Models;

namespace Crypto.Interfaces
{
    public interface IHttpService
    {
        Task<TRes> GetAsync<TRes>(string path);

        Task<TRes> PostAsync<TRes>(string path);
        Task<TRes> PostAsync<TRes, TBody>(string path, TBody body);
        Task<BaseResponse> PostAsync<TBody>(string path, TBody body);
    }
}
