using System;
namespace Gate.Models
{
    public class Product
    {
        public string id { get; set; }                      // "BTC_USDT";
        public string @base { get; set; }                   // "BTC";
        public string quote { get; set; }                   // "USDT";
        public string fee { get; set; }                     // "0.2";
        public string min_base_amount { get; set; } = "0";  // "0.0001";
        public string min_quote_amount { get; set; }        // "1";
        public int amount_precision { get; set; }           // 4;
        public int precision { get; set; }                  // 2;
        public string trade_status { get; set; }            // "tradable";
                                                            //public number sell_start { get; set; }             // 0;
                                                            //public number buy_start { get; set; }              // 0;

        public Common.Models.Product ToCommonProduct()
        {

            return new Common.Models.Product()
            {
                currencyPair = this.id,
                minQuantity = decimal.Parse(this.min_base_amount),
                minTotal = decimal.Parse(this.min_quote_amount),
                pricePrecision = 1 / Math.Pow(10, this.precision),
            };
        }
    }
}

