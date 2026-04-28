using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Responses
{
    public class PortfolioDTO
    {
        public decimal CurrentValue { get; set; } = 0;
        public decimal GainLoss { get; set; } = 0;
        public decimal Performance { get; set; } = 0;
        public List<PositionDTO> Positions { get; set; } = [];
    }
}
