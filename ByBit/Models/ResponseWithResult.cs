using System;
namespace ByBit.Models
{
    public class ResponseWithResult<T> : BaseResponse
    {
        public T result { get; set; }
    }
}

