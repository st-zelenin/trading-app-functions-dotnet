using System.Threading.Tasks;
using Common.Interfaces;

namespace ByBit.Interfaces
{
    public interface IHttpService : IBaseHttpService
    {
        Task<TRes> GetAsync<TRes>(string path);
        Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters);
        //Task<TRes> PostAsync<TRes, TBody>(string path, TBody body);
    }
}
