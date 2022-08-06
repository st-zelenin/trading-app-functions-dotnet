using System;
using CommonTicker = Common.Models.Ticker;

namespace Crypto.Models
{
    public class Ticker
    {
        public string i { get; set; }   //	Instrument Name, e.g. BTC_USDT, ETH_CRO, etc.
        public double b { get; set; }	//  The current best bid price, null if there aren't any bids
        public double k { get; set; }   //	The current best ask price, null if there aren't any asks
        public double a { get; set; }   //	The price of the latest trade, null if there weren't any trades
        public long t { get; set; }     //	Timestamp of the data
        public long v { get; set; }     //	The total 24h traded volume
        public double h { get; set; }   //	Price of the 24h highest trade
        public double l { get; set; }   //	Price of the 24h lowest trade, null if there weren't any trades
        public double c { get; set; }   //	24-hour price change, null if there weren't any trades

        public CommonTicker ToCommonTicker()
        {
            return new CommonTicker()
            {
                last = this.a,
                change_percentage = (this.c / this.a) * 100
            };
        }
    }
}
