using System;
namespace Common.Models
{
    public class Product
    {
        public string currencyPair { get; set; }
        public decimal minQuantity { get; set; }
        public decimal minTotal { get; set; }
        public double pricePrecision { get; set; }
    }
}
