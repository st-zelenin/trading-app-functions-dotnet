namespace DataAccess.Models;

public class Trader
{
    public string id { get; set; }
    public string name { get; set; }
    public IList<CryptoPair> gate { get; set; }
    public IList<CryptoPair> crypto{ get; set; }
    public IList<CryptoPair> coinbase{ get; set; }
    public IList<CryptoPair> bybit { get; set; }
    public IList<CryptoPair> binance { get; set; }
}

public class CryptoPair
{
    public string symbol { get; set; }
    public bool isArchived { get; set; }
}