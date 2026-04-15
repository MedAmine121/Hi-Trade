using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class Transaction : BaseEntity
    {
        public int PortfolioId { get; set; }
        public int AssetId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public TransactionType Type { get; set; }

        public Asset Asset { get; set; } = null!;
    }
}
