using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommonOrder = Common.Models.Order;
using CommonOrderSide = Common.Models.OrderSide;
using CommonOrderStatus = Common.Models.OrderStatus;

namespace DataAccess.Models;

public class GateOrder
{
    public string id { get; set; }
    public long create_time_ms { get; set; }
    public long update_time_ms { get; set; }
    public GateOrderStatus status { get; set; }
    public string currency_pair { get; set; }
    public GateOrderType type { get; set; }
    public GateOrderAccount account { get; set; }
    public GateOrderSide side { get; set; }
    public string amount { get; set; }
    public string price { get; set; }
    public string avg_deal_price { get; set; }
    public string fill_price { get; set; }
    public string filled_total { get; set; }
    public string fee { get; set; }
    public string fee_currency { get; set; }

    public CommonOrder ToCommonOrder()
    {
        return new CommonOrder()
        {
            id = this.id,
            currencyPair = this.currency_pair,
            createTimestamp = this.create_time_ms,
            updateTimestamp = this.update_time_ms,
            side = this.side == GateOrderSide.sell ? CommonOrderSide.sell : CommonOrderSide.buy,
            amount = this.GetCommonOrderAmount(),
            price = this.GetCommonOrderPrice(),
            status = this.ToCommonOrderStatus(),
            type = this.type == GateOrderType.limit ? Common.Models.OrderType.limit : Common.Models.OrderType.market
        };
    }

    private double GetCommonOrderAmount()
    {
        // for limit or market/sell => amount
        if (this.type == GateOrderType.limit || this.side == GateOrderSide.sell)
        {
            return double.Parse(this.amount);
        }

        // market/buy has total in the amount
        return double.Parse(this.amount) / double.Parse(this.avg_deal_price);
    }

    private double GetCommonOrderPrice()
    {
        if (this.type == GateOrderType.limit)
        {
            return double.Parse(this.price);
        }

        return this.avg_deal_price == null ? 0 : double.Parse(this.avg_deal_price);
    }

    private CommonOrderStatus ToCommonOrderStatus()
    {
        switch (this.status)
        {
            case GateOrderStatus.open:
                return CommonOrderStatus.open;
            case GateOrderStatus.closed:
                return CommonOrderStatus.closed;
            case GateOrderStatus.cancelled:
                return CommonOrderStatus.cancelled;
            default:
                throw new ArgumentException($"unhandled gate order status: { status }");
        }
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GateOrderStatus
{
    closed = 1,
    open,
    cancelled,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GateOrderType
{
    limit = 1,
    market,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GateOrderAccount
{
    account = 1,
    spot
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GateOrderSide
{
    sell = 1,
    buy,
}

