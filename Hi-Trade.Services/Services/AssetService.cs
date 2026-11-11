using Azure.Core;
using FluentValidation;
using Hi_Trade.BLL.BLL;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class AssetService(IHiTradeBLL hiTradeBLL, ILogger<AssetService> logger, IServiceProvider serviceProvider) : BaseService(serviceProvider), IAssetService
    {
        public async Task<BaseResult<SaveResponse>> CreateAsset(CreateAssetRequest request, CancellationToken ct)
        {
            try
            {
                SaveResponse? asset = await Validate(hiTradeBLL.CreateAsset!, request, ct);
                if (asset != null)
                {
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
            return await ExecuteAssetQuery(() => hiTradeBLL.GetAllEnabledAssets(ct), "Failed to fetch assets", "An error occurred while fetching assets");
        }
        public async Task<BaseResult<List<AssetDTO>>> GetAllAssets(string? search, bool? enabled, CancellationToken ct)
        {
            return await ExecuteAssetQuery(() => hiTradeBLL.GetAllAssets(search, enabled, ct), "Failed to fetch assets", "An error occurred while fetching all assets");
        }
        public async Task<BaseResult<SaveResponse>> ToggleAssetStatus(int assetId, CancellationToken ct)
        {
            try
            {
                SaveResponse response = await hiTradeBLL.ToggleAssetStatus(assetId, ct);
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
