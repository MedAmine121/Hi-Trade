using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class PortfolioService(IHiTradeBLL hiTradeBLL, ILogger<PortfolioService> logger, IServiceProvider serviceProvider) : BaseService(serviceProvider), IPortfolioService
    {
        public async Task<BaseResult<List<PortfolioDTO>>> GetUserPortfolios(string token, CancellationToken ct)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                token = token.Replace("Bearer ", "");
                var jwtToken = handler.ReadJwtToken(token);
                string? email = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (email == null)
                {
                    throw new Exception("Email not found in JWT token");
                }
                List<PortfolioDTO> portfolios = await hiTradeBLL.GetUserPortfolios(email, ct);
                if (portfolios != null)
                {
                    return new BaseResult<List<PortfolioDTO>>
                    {
                        Model = portfolios,
                        ResultType = ResultType.Success
                    };
                }
                throw new Exception("Portfolios null");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching user portfolios");
                return new BaseResult<List<PortfolioDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
