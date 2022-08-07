
using System;
namespace Crypto.Models
{
    public class ResponseWithResult<T> : BaseResponse
    {
        public T result { get; set; }
    }
}

