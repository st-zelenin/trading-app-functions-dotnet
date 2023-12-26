using System;
using DataAccess.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ByBit.Models;

public class OrderV5
{
    public string id { get; set; }

    public string orderId { get; set; }         // "14bad3a1-6454-43d8-bcf2-5345896cf74d"
    public string orderLinkId { get; set; }     // User customised order ID
    public string symbol { get; set; }          // "BTCUSDT"
    public string price { get; set; }           // "26864.40"
    public string qty { get; set; }             // "0.003"
    public ByBitOrderSide side { get; set; }    // "Buy"
    public ByBitV5OrderStatus orderStatus { get; set; }  // "Cancelled"
    public string avgPrice { get; set; }        // Average filled price. If unfilled, it is ""
    public string cumExecQty { get; set; }      // Cumulative executed order qty
    public string timeInForce { get; set; }     // "PostOnly",
    public ByBitV5OrderType orderType { get; set; }  // "Limit",
    public string createdTime { get; set; }     // "1684476068369",
    public string updatedTime { get; set; }     // "1684476068372"

    public ByBitOrder ToByBitOrder()
    {
        return new ByBitOrder()
        {
            id = this.orderId,
            orderId = this.orderId,
            accountId = string.Empty,
            exchangeId = string.Empty,
            symbol = this.symbol,
            symbolName = this.symbol,
            orderLinkId = this.orderLinkId,
            price = this.price,
            origQty = this.qty,
            executedQty = this.cumExecQty,
            cummulativeQuoteQty = (double.Parse(this.price) * double.Parse(this.cumExecQty)).ToString(),
            avgPrice = this.avgPrice,
            status = this.ToByBitOrderStatus(),
            timeInForce = this.timeInForce,
            type = this.orderType == ByBitV5OrderType.Limit ? ByBitOrderType.LIMIT : ByBitOrderType.MARKET,
            side = this.side,
            stopPrice = string.Empty,
            icebergQty = string.Empty,
            time = this.createdTime,
            updateTime = this.updatedTime,
            isWorking = true,
        };
    }


    private ByBitOrderStatus ToByBitOrderStatus()
    {
        switch (this.orderStatus)
        {
            case ByBitV5OrderStatus.Created:
                return ByBitOrderStatus.PENDING_NEW;
            case ByBitV5OrderStatus.New:
                return ByBitOrderStatus.NEW;
            case ByBitV5OrderStatus.Rejected:
                return ByBitOrderStatus.REJECTED;
            case ByBitV5OrderStatus.PartiallyFilled:
            case ByBitV5OrderStatus.PartiallyFilledCanceled:
                return ByBitOrderStatus.PARTIALLY_FILLED;
            case ByBitV5OrderStatus.Filled:
                return ByBitOrderStatus.FILLED;
            case ByBitV5OrderStatus.Cancelled:
                return ByBitOrderStatus.CANCELED;
            default:
                throw new ArgumentException($"unhandled bybit order status: {orderStatus}");
        }
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ByBitV5OrderStatus
{
    Created = 1,
    New,
    Rejected,
    PartiallyFilled,
    PartiallyFilledCanceled,
    Filled,
    Cancelled,
    Untriggered,
    Triggered,
    Deactivated,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ByBitV5OrderType
{
    Market = 1,
    Limit,
}