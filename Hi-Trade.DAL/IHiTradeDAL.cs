
using Azure.Core;
using Hi_Trade.DAL.Entities;

namespace Hi_Trade.DAL
{
    public interface IHiTradeDAL
    {
        Task<User> CreateUser(string email, string password, string fullName, string address, CancellationToken ct);
        Task<bool> AdminExistsAsync(CancellationToken ct);
        Task<User> CreateAdminUser(string email, string password, string fullName, string address, CancellationToken ct);
        Task<User?> LoginUser(string email, CancellationToken ct);
        Task<Asset> CreateAsset(string ticker, string name, CancellationToken ct);
        Task<List<Asset>> GetAllAssets(CancellationToken ct);
        Task<List<Asset>> GetAssetsFiltered(string? search, bool? enabled, CancellationToken ct);
        Task<Asset> ToggleAssetStatus(int assetId, CancellationToken ct);
        Task UpdateAssetPrice(int assetId, decimal newPrice, CancellationToken ct);
        Task<List<Portfolio>> GetPortfolios(string email, CancellationToken ct);
        Task<(int, string)> CreateUserPortfolio(string name, string email, CancellationToken ct);
        Task<(int, string)> BuyAsset(int portfolioId, int assetId, decimal quantity, string email, CancellationToken ct);
        Task<(int, string)> SellAsset(int portfolioId, int positionId, decimal quantity, string email, CancellationToken ct);
        Task<User> FetchUser(string email, CancellationToken ct);
        Task<(bool, string)> AddFunds(string paymentId, string email, long amount, CancellationToken ct);
        Task<List<Transaction>> GetPortfolioTransactions(int portfolioId,string email, CancellationToken ct);
        Task<User> EditProfile(string oldEmail, string email, string fullName, string address, string profilePictureUrl, CancellationToken ct);
        Task<List<User>> GetUsersFiltered(string? search, Roles? role, bool? enabled, CancellationToken ct);
        Task<User> ToggleUserStatus(int userId, string adminEmail, CancellationToken ct);
        Task<List<Transaction>> GetAllTransactions(string? search, TransactionType? type, CancellationToken ct);
        Task<(int TotalUsers, int TotalTraders, int TotalAdmins, int TotalPortfolios, int TotalAssets, int EnabledAssets, int DisabledAssets, int TotalTransactions, int BuyTransactionsCount, int SellTransactionsCount, decimal TotalTradingVolume, decimal TotalUserBalances, decimal TotalDepositedAmount)> GetPlatformMetrics(CancellationToken ct);
        Task<List<Transaction>> GetRecentTransactions(int count, CancellationToken ct);
        Task<List<User>> GetRecentUsers(int count, CancellationToken ct);
    }
}