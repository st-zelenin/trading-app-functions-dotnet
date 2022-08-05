using System;
using System.Collections.Generic;

namespace Crypto.Models
{
    public class InstrumentsResponseResult
    {
        public IEnumerable<Instrument> instruments { get; set; }
    }

    public class InstrumentsResponse
    {
        public InstrumentsResponseResult result { get; set; }
    }
}

