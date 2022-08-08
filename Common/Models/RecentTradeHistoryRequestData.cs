using System;
namespace Common.Models
{
    public class RecentTradeHistoryRequestData
    {
        public OrderSide side { get; set; }
        public int limit { get; set; }
    }
}

