using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommonOrder = Common.Models.Order;
using CommonOrderSide = Common.Models.OrderSide;
using CommonOrderStatus = Common.Models.OrderStatus;

namespace Crypto.Models
{
    public class Order
    {
        public double avg_price { get; set; }
        public long create_time { get; set; }
        public double cumulative_quantity { get; set; }
        public double cumulative_value { get; set; }
        public string fee_currency { get; set; }
        public string instrument_name { get; set; }
        public string order_id { get; set; }
        public double price { get; set; }
        public double quantity { get; set; }
        public OrderSide side { get; set; }
        public OrderStatus status { get; set; }
        public OrderType type { get; set; }
        public long update_time { get; set; }

        public CommonOrder ToCommonOrder()
        {
            return new CommonOrder()
            {
                id = this.order_id,
                currencyPair = this.instrument_name,
                createTimestamp = this.create_time,
                updateTimestamp = this.update_time,
                side = this.side == OrderSide.BUY ? CommonOrderSide.buy : CommonOrderSide.sell,
                amount = this.quantity > 0 ? this.quantity : this.cumulative_quantity,
                price = this.price > 0 ? this.price : this.avg_price,
                status = this.ToCommonOrderStatus(),
            };
        }

        private CommonOrderStatus ToCommonOrderStatus()
        {
            switch (this.status)
            {
                case OrderStatus.ACTIVE:
                    return CommonOrderStatus.open;
                case OrderStatus.FILLED:
                    return CommonOrderStatus.closed;
                case OrderStatus.CANCELED:
                case OrderStatus.EXPIRED:
                case OrderStatus.REJECTED:
                    return CommonOrderStatus.cancelled;
                default:
                    throw new ArgumentException($"unhandled crypto.com order status: { status }");
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        ACTIVE = 1,
        CANCELED,
        FILLED,
        REJECTED,
        EXPIRED
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        LIMIT = 1
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderSide
    {
        SELL = 1,
        BUY,
    }
}
