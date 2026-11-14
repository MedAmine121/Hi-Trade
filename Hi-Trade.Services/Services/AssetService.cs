using Azure.Core;
using FluentValidation;
using Hi_Trade.BLL.BLL;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Common;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class AssetService(IHiTradeBLL hiTradeBLL, ILogger<AssetService> logger, IServiceProvider serviceProvider, ICacheService? cacheService = null) : BaseService(serviceProvider), IAssetService
    {
        public async Task<BaseResult<SaveResponse>> CreateAsset(CreateAssetRequest request, CancellationToken ct)
        {
            try
            {
                SaveResponse? asset = await Validate(hiTradeBLL.CreateAsset!, request, ct);
                if (asset != null)
                {
                    if (asset.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.EnabledAssets, ct);
                        await cacheService.RemoveByPrefixAsync(CacheKeys.AssetsPrefix, ct);
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
                    Message = "Failed to create asset"
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while creating an asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating an asset");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<List<AssetDTO>>> GetAllEnabledAssets(CancellationToken ct)
        {
            if (cacheService != null)
            {
                var cached = await cacheService.GetAsync<List<AssetDTO>>(CacheKeys.EnabledAssets, ct);
                if (cached != null)
                {
                    return new BaseResult<List<AssetDTO>>
                    {
                        Model = cached,
                        ResultType = ResultType.Success
                    };
                }
            }

            var result = await ExecuteAssetQuery(() => hiTradeBLL.GetAllEnabledAssets(ct), "Failed to fetch assets", "An error occurred while fetching assets");
            if (result.ResultType == ResultType.Success && result.Model != null && cacheService != null)
            {
                await cacheService.SetAsync(CacheKeys.EnabledAssets, result.Model, CacheKeys.DefaultAssetsExpiry, ct);
            }
            return result;
        }
        public async Task<BaseResult<List<AssetDTO>>> GetAllAssets(string? search, bool? enabled, CancellationToken ct)
        {
            var cacheKey = CacheKeys.AllAssets(search, enabled);
            if (cacheService != null)
            {
                var cached = await cacheService.GetAsync<List<AssetDTO>>(cacheKey, ct);
                if (cached != null)
                {
                    return new BaseResult<List<AssetDTO>>
                    {
                        Model = cached,
                        ResultType = ResultType.Success
                    };
                }
            }

            var result = await ExecuteAssetQuery(() => hiTradeBLL.GetAllAssets(search, enabled, ct), "Failed to fetch assets", "An error occurred while fetching all assets");
            if (result.ResultType == ResultType.Success && result.Model != null && cacheService != null)
            {
                await cacheService.SetAsync(cacheKey, result.Model, CacheKeys.DefaultAssetsExpiry, ct);
            }
            return result;
        }
        public async Task<BaseResult<SaveResponse>> ToggleAssetStatus(int assetId, CancellationToken ct)
        {
            try
            {
                SaveResponse response = await hiTradeBLL.ToggleAssetStatus(assetId, ct);
                if (response.Success && cacheService != null)
                {
                    await cacheService.RemoveAsync(CacheKeys.EnabledAssets, ct);
                    await cacheService.RemoveByPrefixAsync(CacheKeys.AssetsPrefix, ct);
                }
                return new BaseResult<SaveResponse>
                {
                    Model = response,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while toggling asset status for asset {AssetId}", assetId);
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        private async Task<BaseResult<List<AssetDTO>>> ExecuteAssetQuery(Func<Task<List<AssetDTO>>> query, string failMessage, string errorLogMessage)
        {
            try
            {
                List<AssetDTO> assets = await query();
                if (assets != null)
                {
                    return new BaseResult<List<AssetDTO>>
                    {
                        Model = assets,
                        ResultType = ResultType.Success
                    };
                }
                return new BaseResult<List<AssetDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = failMessage
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, errorLogMessage);
                return new BaseResult<List<AssetDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
