using CommonTicker = Common.Models.Ticker;

namespace ByBit.Models;

public class Ticker
{
    public string symbol { get; set; } 
    public string lastPrice { get; set; }
    public string price24hPcnt { get; set; }

    public CommonTicker ToCommonTicker()
    {
        return new CommonTicker()
        {
            last = double.Parse(this.lastPrice),
            change_percentage = double.Parse(this.price24hPcnt) * 100
        };
    }
}

