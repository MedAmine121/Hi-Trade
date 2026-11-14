using FluentValidation;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Common;
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
    public class PortfolioService(IHiTradeBLL hiTradeBLL, ILogger<PortfolioService> logger, IServiceProvider serviceProvider, ICacheService? cacheService = null) : BaseService(serviceProvider), IPortfolioService
    {
        public async Task<BaseResult<List<PortfolioDTO>>> GetUserPortfolios(string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                if (cacheService != null)
                {
                    var cachedPortfolios = await cacheService.GetAsync<List<PortfolioDTO>>(CacheKeys.UserPortfolios(email), ct);
                    if (cachedPortfolios != null)
                    {
                        return new BaseResult<List<PortfolioDTO>>
                        {
                            Model = cachedPortfolios,
                            ResultType = ResultType.Success
                        };
                    }
                }

                List<PortfolioDTO> portfolios = await hiTradeBLL.GetUserPortfolios(email, ct);
                if (portfolios != null)
                {
                    if (cacheService != null)
                    {
                        await cacheService.SetAsync(CacheKeys.UserPortfolios(email), portfolios, CacheKeys.DefaultPortfoliosExpiry, ct);
                    }
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
        public async Task<BaseResult<SaveResponse>> CreatePortfolio(CreatePortfolioRequest request,string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.Email = email;
                SaveResponse? asset = await Validate(hiTradeBLL.CreateUserPortfolio!, request, ct);
                if (asset != null)
                {
                    if (asset.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserPortfolios(email), ct);
                    }
                    return new BaseResult<SaveResponse>
                    {
                        Model = asset,
                        ResultType = ResultType.Success
                    };
                }
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = "Failed to create portfolio"
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while creating the portfolio");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the portfolio");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<SaveResponse>> BuyAsset(BuyAssetRequest request, string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.Email = email;
                SaveResponse? asset = await Validate(hiTradeBLL.BuyAsset!, request, ct);
                if (asset != null)
                {
                    if (asset.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserProfile(email), ct);
                        await cacheService.RemoveAsync(CacheKeys.UserPortfolios(email), ct);
                        await cacheService.RemoveAsync(CacheKeys.AdminDashboardStats, ct);
                    }
                    return new BaseResult<SaveResponse>
                    {
                        Model = asset,
                        ResultType = ResultType.Success
                    };
                }
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = "Failed to buy asset"
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while buying the asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while buying the asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<SaveResponse>> SellAsset(SellAssetRequest request, string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.Email = email;
                SaveResponse? asset = await Validate(hiTradeBLL.SellAsset!, request, ct);
                if (asset != null)
                {
                    if (asset.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserProfile(email), ct);
                        await cacheService.RemoveAsync(CacheKeys.UserPortfolios(email), ct);
                        await cacheService.RemoveAsync(CacheKeys.AdminDashboardStats, ct);
                    }
                    return new BaseResult<SaveResponse>
                    {
                        Model = asset,
                        ResultType = ResultType.Success
                    };
                }
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = "Failed to buy asset"
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while buying the asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while buying the asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<List<TransactionDTO>>> GetPortfolioTransactions(GetPortfolioTransactionsRequest request, string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.Email = email;
                List<TransactionDTO>? transactions = await Validate(hiTradeBLL.GetPortfolioTransactions!, request, ct);
                return new BaseResult<List<TransactionDTO>>
                {
                    Model = transactions,
                    ResultType = ResultType.Success
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while fetching transactions");
                return new BaseResult<List<TransactionDTO>>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching portfolio transactions");
                return new BaseResult<List<TransactionDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
