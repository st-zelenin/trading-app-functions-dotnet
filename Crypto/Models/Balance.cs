using System;
using CommonBalance = Common.Models.Balance;

namespace Crypto.Models
{
    public class Balance
    {
        public string  currency { get; set; }
        public double balance { get; set; }
        public double available { get; set; }

        public CommonBalance ToCommonBalance()
        {
            return new CommonBalance()
            {
                available = this.available,
                locked = this.balance - this.available
            };
        }
    }
}

