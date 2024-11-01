using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommonOrder = Common.Models.Order;
using CommonOrderSide = Common.Models.OrderSide;
using CommonOrderStatus = Common.Models.OrderStatus;

namespace DataAccess.Models;

public class CryptoOrder
{
    public string avg_price { get; set; }
    public long create_time { get; set; }
    public string cumulative_quantity { get; set; }
    public string cumulative_value { get; set; }
    public string fee_instrument_name { get; set; }
    public string instrument_name { get; set; }
    public string id { get; set; }
    public string order_id { get; set; }
    public string limit_price { get; set; }
    public string quantity { get; set; }
    public CryptoOrderSide side { get; set; }
    public CryptoOrderStatus status { get; set; }
    public CryptoOrderType type { get; set; }
    public long update_time { get; set; }

    public CommonOrder ToCommonOrder()
    {
        double.TryParse(this.quantity, out var quantity);
        double.TryParse(this.limit_price, out var price);

        return new CommonOrder()
        {
            id = this.order_id,
            currencyPair = this.instrument_name,
            createTimestamp = this.create_time,
            updateTimestamp = this.update_time,
            side = this.side == CryptoOrderSide.BUY ? CommonOrderSide.buy : CommonOrderSide.sell,
            amount = quantity > 0 ? quantity : double.Parse(this.cumulative_quantity),
            price = price > 0 ? price : double.Parse(this.avg_price),
            status = this.ToCommonOrderStatus(),
            type = this.type == CryptoOrderType.LIMIT ? Common.Models.OrderType.limit : Common.Models.OrderType.market
        };
    }

    private CommonOrderStatus ToCommonOrderStatus()
    {
        switch (this.status)
        {
            case CryptoOrderStatus.ACTIVE:
                return CommonOrderStatus.open;
            case CryptoOrderStatus.FILLED:
                return CommonOrderStatus.closed;
            case CryptoOrderStatus.CANCELED:
            case CryptoOrderStatus.EXPIRED:
            case CryptoOrderStatus.REJECTED:
                return CommonOrderStatus.cancelled;
            default:
                throw new ArgumentException($"unhandled crypto.com order status: { status }");
        }
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CryptoOrderStatus
{
    ACTIVE = 1,
    CANCELED,
    FILLED,
    REJECTED,
    EXPIRED
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CryptoOrderType
{
    LIMIT = 1,
    MARKET
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CryptoOrderSide
{
    SELL = 1,
    BUY,
}
