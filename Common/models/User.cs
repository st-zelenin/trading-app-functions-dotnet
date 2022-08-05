using System;
namespace Common.Models
{
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public IEnumerable<string> pairs { get; set; }
        public IEnumerable<string> crypto_pairs { get; set; }
        public IEnumerable<string> coinbase_pairs { get; set; }
        public IEnumerable<string> bybit_pairs { get; set; }
    }
}

