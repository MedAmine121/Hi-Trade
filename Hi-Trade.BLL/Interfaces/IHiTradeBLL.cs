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
    }
}