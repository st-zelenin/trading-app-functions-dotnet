using System.Threading.Tasks;
using Crypto.Models;
using Microsoft.AspNetCore.Http;

namespace Crypto.Interfaces
{
    public interface IHttpService
    {
        Task<T> GetRequestBody<T>(HttpRequest req);

        Task<TRes> GetAsync<TRes>(string path);

        Task<TRes> PostAsync<TRes>(string path);
        Task<TRes> PostAsync<TRes, TBody>(string path, TBody body);
        Task<BaseResponse> PostAsync<TBody>(string path, TBody body);
    }
}
