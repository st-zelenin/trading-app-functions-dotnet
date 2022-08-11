using Common.Models;

namespace ByBit.Models
{
    public class Instrument
    {
        public string name { get; set; }
        public string alias { get; set; }
        public string baseCurrency { get; set; }
        public string quoteCurrency { get; set; }
        public string basePrecision { get; set; }       //	Decimal precision (base currency)
        public string quotePrecision { get; set; }      //	Decimal precision (quote currency)
        public string minTradeQuantity { get; set; }    //	Min. order qty
        public string minTradeAmount { get; set; }      //	Min. order value
        public string minPricePrecision { get; set; }   //	Min. number of decimal places
        public string maxTradeQuantity { get; set; }    //	Max. order qty
        public string maxTradeAmount { get; set; }      //	Max. order value
        public int category { get; set; }

        public Product ToCommonProduct()
        {
            return new Product()
            {
                currencyPair = this.name,
                minQuantity = decimal.Parse(this.minTradeQuantity),
                minTotal = decimal.Parse(this.minTradeAmount),
                pricePrecision = double.Parse(this.minPricePrecision),
            };
        }
    }
}

