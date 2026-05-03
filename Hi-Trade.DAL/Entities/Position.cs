using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class Position : BaseEntity
    {
        public Asset Asset { get; set; } = null!;
        public Portfolio Portfolio { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal AveragePrice { get; set; }
    }
}
