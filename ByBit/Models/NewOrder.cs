using DataAccess.Models;

namespace ByBit.Models
{
    public class BaseNewOrder
    {
        public BaseNewOrder(ByBitOrderType orderType)
        {
            this.type = orderType;
        }

        public string symbol { get; set; }
        public double qty { get; set; }
        public ByBitOrderType type { get; private set; }
        public ByBitOrderSide side { get; set; }
    }

    public class NewLimitOrder : BaseNewOrder
    {
        public NewLimitOrder() : base(ByBitOrderType.LIMIT) { }

        public double price { get; set; }
    }

    public class NewMarketOrder : BaseNewOrder
    {
        public NewMarketOrder() : base(ByBitOrderType.MARKET) { }
    }

    public class BaseNewOrderV5
    {
        public BaseNewOrderV5(ByBitV5OrderType orderType)
        {
            this.orderType = orderType;
        }

        public string category = "spot";

        public string symbol { get; set; }
        public ByBitV5OrderType orderType { get; private set; }
        public ByBitOrderSide side { get; set; }
        public string qty { get; set; }
    }

    public class NewMarketOrderV5 : BaseNewOrderV5
    {
        public NewMarketOrderV5() : base(ByBitV5OrderType.Market) { }
    }

    public class NewLimitOrderV5 : BaseNewOrderV5
    {
        public NewLimitOrderV5() : base(ByBitV5OrderType.Limit) { }

        public string price { get; set; }
    }
}
