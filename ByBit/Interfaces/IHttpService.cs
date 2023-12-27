using System.Threading.Tasks;
using Common.Interfaces;
using ByBit.Models;

namespace ByBit.Interfaces
{
    public interface IHttpService : IBaseHttpService
    {
        Task<TRes> GetUnsignedAsync<TRes, TParams>(string path, TParams parameters);
        Task<TRes> GetAsync<TRes>(string path);
        Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters);
        Task<TRes> PostAsync<TRes>(string path, string serializedBody);
    }
}
