using System;
using System.Collections.Generic;

namespace Crypto.Models
{
    public class InstrumentsResponseResult
    {
        public IEnumerable<Instrument> instruments { get; set; }
    }

    public class OrdersResponseResult
    {
        public IEnumerable<Order> order_list { get; set; }

        public long count { get; set; }
    }
}

