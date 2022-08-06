using System;
using Common.Models;

namespace Crypto.Models
{
    public class Instrument
    {
        public string instrument_name { get; set; }
        public string quote_currency { get; set; }
        public string base_currency { get; set; }
        public double price_decimals { get; set; }
        public decimal quantity_decimals { get; set; }
        public string min_quantity { get; set; }

        public Product ToCommonProduct()
        {
            return new Product()
            {
                currencyPair = this.instrument_name,
                minQuantity = decimal.Parse(this.min_quantity),
                minTotal = 0,
                pricePrecision = 1 / Math.Pow(10, this.price_decimals),
            };
        }
    }
}

