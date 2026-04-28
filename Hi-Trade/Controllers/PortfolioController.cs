using Hi_Trade.Models.Common;
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
    }
}
