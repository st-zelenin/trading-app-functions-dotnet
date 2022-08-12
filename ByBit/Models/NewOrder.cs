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

    public class NewLimitOrder: BaseNewOrder
    {
        public NewLimitOrder() : base(ByBitOrderType.LIMIT) { }

        public double price { get; set; }
    }

    public class NewMarketOrder : BaseNewOrder
    {
        public NewMarketOrder() : base(ByBitOrderType.MARKET) { }
    }
}
