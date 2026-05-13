using Hi_Trade.Models.Common;

namespace Hi_Trade.Models.Responses
{
    public class TransactionDTO : BaseDTO
    {
        public int AssetId { get; set; }
        public string AssetTicker { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PriceAtTransaction { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalValue { get; set; }
    }
}
