
using Hi_Trade.DAL.Entities;

namespace Hi_Trade.DAL
{
    public interface IHiTradeDAL
    {
        Task<User> CreateUser(string email, string password, string fullName, string address, CancellationToken ct);
        Task<User?> LoginUser(string email, CancellationToken ct);
        Task<Asset> CreateAsset(string ticker, string name, CancellationToken ct);
        Task<List<Asset>> GetAllAssets(CancellationToken ct);
        Task UpdateAssetPrice(int assetId, decimal newPrice, CancellationToken ct);
        Task<List<Portfolio>> GetPortfolios(string email, CancellationToken ct);
        Task<(int, string)> CreateUserPortfolio(string name, string email, CancellationToken ct);
    }
}