using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class Portfolio : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public List<Transaction> Transactions { get; set; } = new();
        public decimal TotalRealizedGainLoss { get; set; } = 0;
        public decimal NetInvested { get; set; } = 0;
        public List<Position> Positions { get; set; } = [];
    }
}
