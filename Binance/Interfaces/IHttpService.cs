using System;
using System.Threading.Tasks;
using Common.Interfaces;

namespace Binance.Interfaces;

public interface IHttpService : IBaseHttpService
{
    Task<TRes> GetAsync<TRes>(string path);
    Task<TRes> GetAsync<TRes, TParams>(string path, TParams parameters);

    Task<TRes> GetSignedAsync<TRes>(string path);
    Task<TRes> GetSignedAsync<TRes, TParams>(string path, TParams parameters);

    Task<TRes> PostSignedAsync<TRes>(string path);
    Task<TRes> PostSignedAsync<TRes, TParams>(string path, TParams parameters);

    Task<TRes> DeleteSignedAsync<TRes, TParams>(string path, TParams parameters);
}

