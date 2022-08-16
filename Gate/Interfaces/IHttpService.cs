using System;
using System.Threading.Tasks;
using Common.Interfaces;

namespace Gate.Interfaces
{
    public interface IHttpService : IBaseHttpService
    {
        Task<TRes> GetAsync<TRes>(string path);
        Task<TRes> GetAsync<TRes, TQuery>(string path, TQuery query);
        Task<TRes> PostAsync<TRes, TBody>(string path, TBody body);
        Task<TRes> PostAsync<TRes, TBody, TQuery>(string path, TBody body, TQuery query);
        Task<TRes> DeleteAsync<TRes, TQuery>(string path, TQuery query);
    }
}
