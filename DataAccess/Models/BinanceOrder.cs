using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommonOrder = Common.Models.Order;
using CommonOrderSide = Common.Models.OrderSide;
using CommonOrderStatus = Common.Models.OrderStatus;

namespace DataAccess.Models;

public class BinanceOrder
{
    public string id { get; set; }

    public string symbol { get; set; }              // "LTCBTC",
    public string orderId { get; set; }             // 1,
    public string orderListId { get; set; }         // -1, //Unless OCO, the value will always be -1
    public string clientOrderId { get; set; }       // "myOrder1",
    public string price { get; set; }               // "0.1",
    public string origQty { get; set; }             // "1.0",
    public string executedQty { get; set; }         // "0.0",
    public string cummulativeQuoteQty { get; set; } // "0.0",
    public BinanceOrderStatus status { get; set; }  // "NEW",
    public string timeInForce { get; set; }         // "GTC",
    public BinanceOrderType type { get; set; }      // "LIMIT",
    public BinanceOrderSide side { get; set; }      // "BUY",
    public string stopPrice { get; set; }           // "0.0",
    public string icebergQty { get; set; }          // "0.0",
    public string time { get; set; }                // 1499827319559,
    public string updateTime { get; set; }          // 1499827319559,
    public string isWorking { get; set; }           // true,
    public string origQuoteOrderQty { get; set; }   // "0.000000",
    public string workingTime { get; set; }         // 1499827319559,
    public string selfTradePreventionMode { get; set; } // "NONE"

    public CommonOrder ToCommonOrder()
    {
        double amount;
        if (!double.TryParse(this.executedQty, out amount) || amount <= 0)
        {
            amount = double.Parse(this.origQty);
        }

        return new CommonOrder()
        {
            id = this.orderId,
            currencyPair = this.symbol,
            createTimestamp = long.Parse(this.time),
            updateTimestamp = long.Parse(this.updateTime),
            side = this.side == BinanceOrderSide.BUY ? CommonOrderSide.buy : CommonOrderSide.sell,
            amount = amount,
            price = double.Parse(this.price),
            status = this.ToCommonOrderStatus(),
            type = this.type == BinanceOrderType.LIMIT ? Common.Models.OrderType.limit : Common.Models.OrderType.market
        };
    }


    private CommonOrderStatus ToCommonOrderStatus()
    {
        switch (this.status)
        {
            case BinanceOrderStatus.NEW:
            case BinanceOrderStatus.PARTIALLY_FILLED:
                return CommonOrderStatus.open;
            case BinanceOrderStatus.FILLED:
                return CommonOrderStatus.closed;
            case BinanceOrderStatus.REJECTED:
            case BinanceOrderStatus.PENDING_CANCEL:
            case BinanceOrderStatus.CANCELED:
            case BinanceOrderStatus.EXPIRED:
                return CommonOrderStatus.cancelled;
            default:
                throw new ArgumentException($"unhandled binance order status: {status}");
        }
    }
}


[JsonConverter(typeof(StringEnumConverter))]
public enum BinanceOrderStatus
{
    NEW = 1,
    PARTIALLY_FILLED,
    FILLED,
    CANCELED,
    PENDING_CANCEL,
    REJECTED,
    EXPIRED
}

[JsonConverter(typeof(StringEnumConverter))]
public enum BinanceOrderType
{
    LIMIT = 1,
    MARKET
}

[JsonConverter(typeof(StringEnumConverter))]
public enum BinanceOrderSide
{
    BUY,
    SELL,
}