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

    public string timeInForce { get; set; } = "GTC";
    public decimal quantity { get; set; }
    public decimal price { get; set; }
}

public class NewMarketOrder : BaseNewOrder
{
    public NewMarketOrder() : base(BinanceOrderType.MARKET) { }

    public decimal quoteOrderQty { get; set; }
}
