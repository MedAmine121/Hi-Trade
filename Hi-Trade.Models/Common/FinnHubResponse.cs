using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Common
{
    public class FinnHubResponse
    {
        public decimal c { get; set; } // Current price
        public decimal h { get; set; } // High price of the day
        public decimal l { get; set; } // Low price of the day
        public decimal o { get; set; } // Open price of the day
        public decimal pc { get; set; } // Previous close price
        public int t { get; set; } // Timestamp of the data
    }
}
