using Hi_Trade.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Responses
{
    public class PositionDTO : BaseDTO
    {
        public AssetDTO Asset { get; set; } = new();
        public decimal Quantity { get; set; } = 0;
        public decimal AveragePrice { get; set; } = 0;
    }
}
