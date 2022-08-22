using System;
namespace Gate.Models
{
    public class Ticker
    {
        public string currency_pair { get; set; }
        public string last { get; set; }
        public string lowest_ask { get; set; }
        public string highest_bid { get; set; }
        public string change_percentage { get; set; }
        public string base_volume { get; set; }
        public string quote_volume { get; set; }
        public string high_24h { get; set; }
        public string low_24h { get; set; }

        public Common.Models.Ticker ToCommonTicker()
        {
            return new Common.Models.Ticker()
            {
                last = string.IsNullOrEmpty(this.last) ? 0 : double.Parse(this.last),
                change_percentage = string.IsNullOrEmpty(this.change_percentage) ? 0 : double.Parse(this.change_percentage)
            };
        }
    }
}

