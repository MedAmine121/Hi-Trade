using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hi_Trade.Controllers
{
    [Route("portfolio")]
    public class PortfolioController(IPortfolioService portfolioService) : Controller
    {
        [HttpGet("get")]
        [Authorize]
        public async Task<BaseResult<List<PortfolioDTO>>> GetUserPortfolios([FromHeader(Name = "Authorization")] string token, CancellationToken ct)
        {
            return await portfolioService.GetUserPortfolios(token, ct);
        }
        [HttpPost("create")]
        [Authorize]
        public async Task<BaseResult<SaveResponse>> CreatePortfolio([FromHeader(Name = "Authorization")] string token, [FromBody] CreatePortfolioRequest request, CancellationToken ct)
        {
            return await portfolioService.CreatePortfolio(request, token, ct);
        }
        [HttpPost("buyasset")]
        [Authorize]
        public async Task<BaseResult<SaveResponse>> BuyAsset([FromHeader(Name = "Authorization")] string token, [FromBody] BuyAssetRequest request, CancellationToken ct)
        {
            return await portfolioService.BuyAsset(request, token, ct);
        }
    }
}
