using System;
namespace Crypto.Models
{
    public class BaseResponse
    {
        public int code { get; set; }
        public long id { get; set; }
        public string method { get; set; }
    }
}

