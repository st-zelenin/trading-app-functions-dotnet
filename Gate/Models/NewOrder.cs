using Common.Models;
using DataAccess.Models;
using CommonNewOrder = Common.Models.NewOrder;

namespace Gate.Models
{
    public class NewOrder
    {
        public string currency_pair { get; set; }
        public string amount { get; set; }
        public string price { get; set; }
        public GateOrderSide side { get; set; }

        public static NewOrder FromCommonNewOrder(CommonNewOrder order)
        {
            return new NewOrder()
            {
                currency_pair = order.currencyPair,
                side = order.side == CommonOrderSides.buy ? GateOrderSide.buy : GateOrderSide.sell,
                price = order.price,
                amount = order.amount,
            };
        }
    }
}
