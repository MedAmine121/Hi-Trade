using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.Services.Interfaces
{
    public interface IPortfolioService
    {
        Task<BaseResult<List<PortfolioDTO>>> GetUserPortfolios(string token, CancellationToken ct);
        Task<BaseResult<SaveResponse>> CreatePortfolio(CreatePortfolioRequest request, string token, CancellationToken ct);
    }
}