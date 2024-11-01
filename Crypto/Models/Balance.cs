using System.Collections.Generic;
using CommonBalance = Common.Models.Balance;

namespace Crypto.Models;

public class Balance
{
    public IEnumerable<PositionBalance> position_balances { get; set; }
}

public class PositionBalance
{
    public string instrument_name { get; set; }
    public string quantity { get; set; }
    public string reserved_qty { get; set; }

    public CommonBalance ToCommonBalance()
    {
        double.TryParse(this.reserved_qty, out var reserved);

        return new CommonBalance()
        {
            available = double.Parse(this.quantity) - reserved,
            locked = reserved
        };
    }
}
