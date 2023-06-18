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
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CommonOrderSides
{
    buy = 1,
    sell
}

