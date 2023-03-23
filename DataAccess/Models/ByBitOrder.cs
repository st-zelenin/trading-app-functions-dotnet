using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommonOrder = Common.Models.Order;
using CommonOrderSide = Common.Models.OrderSide;
using CommonOrderStatus = Common.Models.OrderStatus;

namespace DataAccess.Models;

public class ByBitOrder
{
    public string id { get; set; }

    public string orderId { get; set; }                 // "1077206026897467392"
    public string accountId { get; set; }               // "17013529",
    public string exchangeId { get; set; }              // "301",
    public string symbol { get; set; }                  // "BITUSDT",
    public string symbolName { get; set; }              // "BITUSDT",
    public string orderLinkId { get; set; }             // "1643148964546",
    public string price { get; set; }                   // "1.5",
    public string origQty { get; set; }                 // "10",
    public string executedQty { get; set; }             // "10",
    public string cummulativeQuoteQty { get; set; }     // "15",
    public string avgPrice { get; set; }                // "1.5",
    public ByBitOrderStatus status { get; set; }        // "FILLED",
    public string timeInForce { get; set; }             // "GTC",
    public ByBitOrderType type { get; set; }            // "LIMIT",
    public ByBitOrderSide side { get; set; }            // "BUY",
    public string stopPrice { get; set; }               // "0.0",
    public string icebergQty { get; set; }              // "0.0",
    public string time { get; set; }                    // "1643148965166",
    public string updateTime { get; set; }              // "1643179683202,
    public bool isWorking { get; set; }

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
            side = this.side == ByBitOrderSide.Buy ? CommonOrderSide.buy : CommonOrderSide.sell,
            amount = amount,
            price = this.type == ByBitOrderType.MARKET ? double.Parse(this.avgPrice) : double.Parse(this.price),
            status = this.ToCommonOrderStatus(),
            type = this.type == ByBitOrderType.LIMIT ? Common.Models.OrderType.limit : Common.Models.OrderType.market
        };
    }

    private CommonOrderStatus ToCommonOrderStatus()
    {
        switch (this.status)
        {
            case ByBitOrderStatus.NEW:
            case ByBitOrderStatus.PARTIALLY_FILLED:
            case ByBitOrderStatus.PENDING_NEW:
                return CommonOrderStatus.open;
            case ByBitOrderStatus.FILLED:
                return CommonOrderStatus.closed;
            case ByBitOrderStatus.REJECTED:
            case ByBitOrderStatus.PENDING_CANCEL:
                return CommonOrderStatus.cancelled;
            case ByBitOrderStatus.CANCELED:
                return type == ByBitOrderType.MARKET ? CommonOrderStatus.closed : CommonOrderStatus.cancelled; // TODO: strange bybit behavior
            default:
                throw new ArgumentException($"unhandled bybit order status: { status }");
        }
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ByBitOrderStatus
{
    NEW = 1,
    PARTIALLY_FILLED,
    FILLED,
    CANCELED,
    PENDING_CANCEL,
    PENDING_NEW,
    REJECTED,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ByBitOrderType
{
    LIMIT = 1,
    MARKET
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ByBitOrderSide
{
    Sell = 1,
    Buy,
}

