using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;

namespace Hi_Trade.Services.Interfaces
{
    public interface IAssetService
    {
        Task<BaseResult<SaveResponse>> CreateAsset(CreateAssetRequest request, CancellationToken ct);
    }
}