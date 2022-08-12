using System;
namespace ByBit.Models
{
    public class Balance
    {
        public string coin { get; set; }
        public string coinId { get; set; }
        public string coinName { get; set; }
        public string total { get; set; }
        public string free { get; set; }
        public string locked { get; set; }
    }
}

