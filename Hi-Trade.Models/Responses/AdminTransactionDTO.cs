using Hi_Trade.Models.Common;
using System;

namespace Hi_Trade.Models.Responses
{
    public class AdminTransactionDTO : BaseDTO
    {
        public DateTime TransactionDate { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string PortfolioName { get; set; } = string.Empty;
        public string AssetTicker { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
