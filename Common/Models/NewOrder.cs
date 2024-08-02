using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Models;

public class NewOrder
{
    public string currencyPair { get; set; }
    public CommonOrderSides side { get; set; }
    public string amount { get; set; }
    public string price { get; set; }
    public string total { get; set; }
    public bool? market { get; set; }

    public static Order ToNewDexOrder(NewOrder order, string associatedCex)
    {
        var unixTimeMilliseconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        return new Order()
        {
            id = Guid.NewGuid().ToString(),
            currencyPair = order.currencyPair,
            createTimestamp = unixTimeMilliseconds,
            updateTimestamp = unixTimeMilliseconds,
            side = order.side == CommonOrderSides.buy ? OrderSide.buy : OrderSide.sell,
            amount = double.Parse(order.amount),
            price = double.Parse(order.price),
            status = OrderStatus.closed,
            type = OrderType.market,
            associatedCex = associatedCex,
            isDex = true
        };
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CommonOrderSides
{
    buy = 1,
    sell
}

