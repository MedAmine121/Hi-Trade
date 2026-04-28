using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Responses
{
    public class AssetDTO
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; } = 0;
    }
}
