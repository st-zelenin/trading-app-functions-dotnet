using System.Threading.Tasks;
using Common.Interfaces;
using ByBit.Models;

namespace ByBit.Interfaces
{
    public interface IHttpService : IBaseHttpService
    {
        Task<TRes> GetAsync<TRes>(string path);
        Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters);
        Task<TRes> GetV5Async<TRes>(string path);
        Task<TRes> GetV5Async<TRes, TParams>(string path, TParams parameters);
        Task<TRes> DeleteAsync<TRes, TParams>(string path, TParams parameters);
        Task<BaseResponse> PostAsync<TBody>(string path, TBody body);
    }
}
