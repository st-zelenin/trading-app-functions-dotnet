using System;
using System.Threading.Tasks;

namespace Crypto.Interfaces
{
    public interface IHttpService
    {
        //Task<TRes> Post<TRes>(string path, object body);
        Task<TRes> Post<TRes, TBody>(string path, TBody body) where TBody : new();
    }
}

