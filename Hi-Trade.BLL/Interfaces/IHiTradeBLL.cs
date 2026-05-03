using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.BLL.Interfaces
{
    public interface IHiTradeBLL
    {
        Task<UserDTO> CreateUser(CreateUserRequest request, CancellationToken ct);
        Task<UserDTO?> LoginUser(LoginUserRequest request, CancellationToken ct);
        Task<SaveResponse> CreateAsset(CreateAssetRequest request, CancellationToken ct);
        Task UpdateAssetPrices(CancellationToken ct);
        Task<List<PortfolioDTO>> GetUserPortfolios(string email, CancellationToken ct);
        Task<SaveResponse> CreateUserPortfolio(CreatePortfolioRequest request, CancellationToken ct);
        Task<List<AssetDTO>> GetAllEnabledAssets(CancellationToken ct);
        Task<SaveResponse> BuyAsset(BuyAssetRequest request, CancellationToken ct);
        Task<SaveResponse> SellAsset(SellAssetRequest request, CancellationToken ct);
    }
}