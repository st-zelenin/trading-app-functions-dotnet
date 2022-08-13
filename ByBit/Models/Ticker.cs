using CommonTicker = Common.Models.Ticker;

namespace ByBit.Models
{
    public class Ticker
    {
        public long time { get; set; }         //	Current timestamp, unit in millisecond
        public string symbol { get; set; }       //	Name of the trading pair
        public string bestBidPrice { get; set; } //	Best bid price
        public string bestAskPrice { get; set; } //	Best ask price
        public string lastPrice { get; set; }    //	Last traded price
        public string openPrice { get; set; }    //	Open price
        public string highPrice { get; set; }    //	High price
        public string lowPrice { get; set; }     //	Low price
        public string volume { get; set; }       //	Trading volume
        public string quoteVolume { get; set; }  //	Trading quote volume

        public CommonTicker ToCommonTicker()
        {
            var lastPrice = double.Parse(this.lastPrice);
            var openPrice = double.Parse(this.openPrice);

            return new CommonTicker()
            {
                last = lastPrice,
                change_percentage = (lastPrice - openPrice) / openPrice * 100
            };
        }
    }
}

