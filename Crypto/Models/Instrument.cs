using System;
using Common.Models;

namespace Crypto.Models
{
    public class Instrument
    {
        public string symbol { get; set; }
        public string quote_currency { get; set; }
        public string base_ccy { get; set; }
        public int quote_decimals { get; set; }
        public int quantity_decimals { get; set; }
        public string qty_tick_size { get; set; }

        public Product ToCommonProduct()
        {
            return new Product()
            {
                currencyPair = this.symbol,
                minQuantity = decimal.Parse(this.qty_tick_size),
                minTotal = 0,
                pricePrecision = 1 / Math.Pow(10, this.quote_decimals),
            };
        }
    }
}

