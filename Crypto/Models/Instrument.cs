using System;
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
    }
}

