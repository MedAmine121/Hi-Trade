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
            try
            {
                List<AssetDTO> assets = await hiTradeBLL.GetAllEnabledAssets(ct);
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
                    Message = "Failed to fetch assets"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching assets");
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
