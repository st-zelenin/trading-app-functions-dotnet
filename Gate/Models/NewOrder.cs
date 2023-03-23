using Common.Models;
using DataAccess.Models;
using CommonNewOrder = Common.Models.NewOrder;

namespace Gate.Models;

public class BaseNewOrder
{
    public BaseNewOrder(GateOrderType orderType)
    {
        this.type = orderType;
    }

    public string currency_pair { get; set; }
    public string amount { get; set; }
    public GateOrderType type { get; private set; }
    public GateOrderSide side { get; set; }

    //public static BaseNewOrder FromCommonNewOrder(CommonNewOrder order)
    //{
    //    return new BaseNewOrder()
    //    {
    //        currency_pair = order.currencyPair,
    //        side = order.side == CommonOrderSides.buy ? GateOrderSide.buy : GateOrderSide.sell,
    //        price = order.price,
    //        amount = order.amount,
    //    };
    //}
}

public class NewLimitOrder : BaseNewOrder
{
    public NewLimitOrder() : base(GateOrderType.limit) { }

    public double price { get; set; }
}

public class NewMarketOrder : BaseNewOrder
{
    public NewMarketOrder() : base(GateOrderType.market) { }

    public string time_in_force { get; } = "fok";
}