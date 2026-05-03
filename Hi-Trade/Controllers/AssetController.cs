using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hi_Trade.Controllers
{
    [Route("asset")]
    public class AssetController(IAssetService assetService) : Controller
    {
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<BaseResult<SaveResponse>> CreateAsset([FromBody] CreateAssetRequest request, CancellationToken ct)
        {
            return await assetService.CreateAsset(request, ct);
        }
        [HttpGet("get")]
        [Authorize]
        public async Task<BaseResult<List<AssetDTO>>> GetEnabledAssets(CancellationToken ct)
        {
            return await assetService.GetAllEnabledAssets(ct);
        }
    }
}
