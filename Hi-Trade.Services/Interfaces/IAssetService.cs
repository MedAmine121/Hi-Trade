using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.Services.Interfaces
{
    public interface IAssetService
    {
        Task<BaseResult<SaveResponse>> CreateAsset(CreateAssetRequest request, CancellationToken ct);
        Task<BaseResult<List<AssetDTO>>> GetAllEnabledAssets(CancellationToken ct);
    }
}