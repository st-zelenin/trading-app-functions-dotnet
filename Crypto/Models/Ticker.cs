using System;
using CommonTicker = Common.Models.Ticker;

namespace Crypto.Models
{
    public class Ticker
    {
        public string i { get; set; }   //	Instrument Name, e.g. BTC_USDT, ETH_CRO, etc.
        public string b { get; set; }	//  The current best bid price, null if there aren't any bids
        public string k { get; set; }   //	The current best ask price, null if there aren't any asks
        public string a { get; set; }   //	The price of the latest trade, null if there weren't any trades
        public long t { get; set; }     //	Timestamp of the data
        public string v { get; set; }     //	The total 24h traded volume
        public string h { get; set; }   //	Price of the 24h highest trade
        public string l { get; set; }   //	Price of the 24h lowest trade, null if there weren't any trades
        public string c { get; set; }   //	24-hour price change, null if there weren't any trades

        public CommonTicker ToCommonTicker()
        {
            var last = double.Parse(this.a);
            return new CommonTicker()
            {
                last = last,
                change_percentage = double.Parse(this.c) * 100
            };
        }
    }
}
