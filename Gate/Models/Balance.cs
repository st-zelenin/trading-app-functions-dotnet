using CommonBalance = Common.Models.Balance;

namespace Gate.Models
{
    public class Balance
    {
        public string currency { get; set; }
        public string locked { get; set; }
        public string available { get; set; }

        public CommonBalance ToCommonBalance()
        {
            return new CommonBalance()
            {
                available = double.Parse(this.available),
                locked = double.Parse(this.locked)
            };
        }
    }
}
