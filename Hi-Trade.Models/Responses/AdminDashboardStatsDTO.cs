using System.Collections.Generic;

namespace Hi_Trade.Models.Responses
{
    public class AdminDashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalTraders { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalPortfolios { get; set; }
        public int TotalAssets { get; set; }
        public int EnabledAssets { get; set; }
        public int DisabledAssets { get; set; }
        public int TotalTransactions { get; set; }
        public int BuyTransactionsCount { get; set; }
        public int SellTransactionsCount { get; set; }
        public decimal TotalTradingVolume { get; set; }
        public decimal TotalUserBalances { get; set; }
        public decimal TotalDepositedAmount { get; set; }
        public List<AdminTransactionDTO> RecentTransactions { get; set; } = [];
        public List<AdminUserDTO> RecentUsers { get; set; } = [];
    }
}
