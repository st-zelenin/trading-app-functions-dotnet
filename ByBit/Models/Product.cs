using System;
using CommonProduct = Common.Models.Product;

namespace ByBit.Models
{
    public class Product
    {
        public string name { get; set; }                //                                      "BTCUSDT"
        public string alias { get; set; }               //                                      "BTCUSDT"
        public string baseCurrency { get; set; }        //                                      "BTC"
        public string quoteCurrency { get; set; }       //                                      "USDT"
        public string basePrecision { get; set; }       //	Decimal precision (base currency)   "0.000001"
        public string quotePrecision { get; set; }      //	Decimal precision (quote currency)  "0.00000001"
        public string minTradeQuantity { get; set; }    //	Min. order qty                      "0.00004"
        public string minTradeAmount { get; set; }      //	Min. order value                    "1"
        public string minPricePrecision { get; set; }   //	Min. number of decimal places       "0.01"
        public string maxTradeQuantity { get; set; }    //	Max. order qty                      "46.13"
        public string maxTradeAmount { get; set; }      //	Max. order value                    "820000"
        public string category { get; set; }            //                                      1

        public CommonProduct ToCommonProduct()
        {
            return new CommonProduct()
            {
                currencyPair = this.name,
                minQuantity = decimal.Parse(this.minTradeQuantity),
                minTotal = decimal.Parse(this.minTradeAmount),
                pricePrecision = double.Parse(this.minPricePrecision),
            };
        }
    }
}

