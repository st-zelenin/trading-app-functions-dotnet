using System;
using CommonTicker = Common.Models.Ticker;

namespace Binance.Models;

public class Ticker
{
    public string symbol { get; set; }                  // "BNBBTC",
    //public string priceChange { get; set; }             // "-94.99999800",
    public string priceChangePercent { get; set; }      // "-95.960",
    //public string weightedAvgPrice { get; set; }        // "0.29628482",
    //public string prevClosePrice { get; set; }          // "0.10002000",
    public string lastPrice { get; set; }               // "4.00000200",
    //public string lastQty { get; set; }                 // "200.00000000",
    //public string bidPrice { get; set; }                // "4.00000000",
    //public string bidQty { get; set; }                  // "100.00000000",
    //public string askPrice { get; set; }                // "4.00000200",
    //public string askQty { get; set; }                  // "100.00000000",
    //public string openPrice { get; set; }               // "99.00000000",
    //public string highPrice { get; set; }               // "100.00000000",
    //public string lowPrice { get; set; }                // "0.10000000",
    //public string volume { get; set; }                  // "8913.30000000",
    //public string quoteVolume { get; set; }             // "15.30000000",
    //public string openTime { get; set; }                // 1499783499040,
    //public string closeTime { get; set; }               // 1499869899040,
    //public string firstId { get; set; }                 // 28385,       // First tradeId
    //public string lastId { get; set; }                  // 28460,       // Last tradeId
    //public string count { get; set; }                   // 76           // Trade count

    public CommonTicker ToCommonTicker()
    {
        return new CommonTicker
        {
            last = double.Parse(this.lastPrice),
            change_percentage = double.Parse(this.priceChangePercent)
        };
    }
}

