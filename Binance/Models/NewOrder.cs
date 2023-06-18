using DataAccess.Models;

namespace Binance.Models;

public class BaseNewOrder
{
    public BaseNewOrder(BinanceOrderType orderType)
    {
        this.type = orderType;
    }

    public string symbol { get; set; }
    public BinanceOrderSide side { get; set; }
    public BinanceOrderType type { get; private set; }
}

public class NewLimitOrder: BaseNewOrder
{
    public NewLimitOrder() : base(BinanceOrderType.LIMIT) { }
    public decimal quantity { get; set; }
    public string timeInForce { get; set; } = "GTC";
    public decimal price { get; set; }
}

public class NewMarketCoinsQuantityOrder : BaseNewOrder
{
    public NewMarketCoinsQuantityOrder() : base(BinanceOrderType.MARKET) { }
    public decimal quantity { get; set; }
}

public class NewMarketTotalMoneyOrder : BaseNewOrder
{
    public NewMarketTotalMoneyOrder() : base(BinanceOrderType.MARKET) { }
    public decimal quoteOrderQty { get; set; }
}
