using System;
namespace DataAccess.Models
{
    public class CryptoAverage
    {
        public double total_money { get; set; }
        public double total_volume { get; set; }
        public string side { get; set; }
        public string currency_pair { get; set; }
    }
}

