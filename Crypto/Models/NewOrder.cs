﻿using DataAccess.Models;

namespace Crypto.Models
{
    public class BaseNewOrder
    {
        public BaseNewOrder(CryptoOrderType orderType)
        {
            this.type = orderType;
        }

        public string instrument_name { get; set; }
        public CryptoOrderType type { get; private set; }
        public CryptoOrderSide side { get; set; }
    }

    public class NewLimitOrder: BaseNewOrder
    {
        public NewLimitOrder() : base(CryptoOrderType.LIMIT) { }

        public string price { get; set; }
        public string quantity { get; set; }
    }

    public class NewMarketOrder : BaseNewOrder
    {
        public NewMarketOrder() : base(CryptoOrderType.MARKET) { }

        public string? quantity { get; set; }
        public string? notional { get; set; }
    }
}
