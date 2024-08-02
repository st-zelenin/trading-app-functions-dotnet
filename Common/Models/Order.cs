using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Models;

public class Order
{
    public string id { get; set; }
    public string currencyPair { get; set; }
    public long createTimestamp { get; set; }
    public long updateTimestamp { get; set; }
    public OrderSide side { get; set; }
    public double amount { get; set; }
    public double price { get; set; }
    public OrderStatus status { get; set; }
    public OrderType type { get; set; }
    public string associatedCex { get; set; } = string.Empty;
    public bool isDex { get; set; } = false;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OrderSide
{
    buy = 1,
    sell
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OrderStatus
{
    closed = 1,
    open,
    cancelled
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OrderType
{
    limit = 1,
    market
}

