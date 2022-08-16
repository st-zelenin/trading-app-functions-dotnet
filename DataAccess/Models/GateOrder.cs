using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAccess.Models
{
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
        public string fill_price { get; set; }
        public string filled_total { get; set; }
        public string fee { get; set; }
        public string fee_currency { get; set; }
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
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GateOrderAccount
    {
        account = 1,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GateOrderSide
    {
        sell = 1,
        buy,
    }
}

