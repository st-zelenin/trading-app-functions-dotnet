using System;
namespace Common.Interfaces
{
    public class User
    {
        string id { get; set; }
        string name { get; set; }
        string[] pairs { get; set; }
        string[] crypto_pairs { get; set; }
        string[] coinbase_pairs { get; set; }
        string[] bybit_pairs { get; set; }
    }
}

