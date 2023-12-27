using DataAccess.Models;

namespace ByBit.Models
{
    public class BaseNewOrder
    {
        public BaseNewOrder(ByBitV5OrderType orderType)
        {
            this.orderType = orderType;
        }

        public string category = "spot";

        public string symbol { get; set; }
        public ByBitV5OrderType orderType { get; private set; }
        public ByBitOrderSide side { get; set; }
        public string qty { get; set; }
    }

    public class NewMarketOrder : BaseNewOrder
    {
        public NewMarketOrder() : base(ByBitV5OrderType.Market) { }
    }

    public class NewLimitOrder : BaseNewOrder
    {
        public NewLimitOrder() : base(ByBitV5OrderType.Limit) { }

        public string price { get; set; }
    }
}
