using FluentValidation;
using Hi_Trade.BLL.Interfaces;
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
    public class UserService(IHiTradeBLL hiTradeBLL, ITokenBLL tokenBLL, ILogger<UserService> logger, IServiceProvider serviceProvider) : BaseService(serviceProvider), IUserService
    {
        public async Task<BaseResult<UserDTO>> CreateUser(CreateUserRequest request, CancellationToken ct)
        {
            try
            {
                UserDTO? user = await Validate(hiTradeBLL.CreateUser!, request, ct);
                if (user != null)
                {
                    user = tokenBLL.GenerateJwtToken(user);
                }
                return new BaseResult<UserDTO>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while creating an asset");
                return new BaseResult<UserDTO>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a user");
                return new BaseResult<UserDTO>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<UserDTO>> LoginUser(LoginUserRequest request, CancellationToken ct)
        {
            try
            {
                UserDTO? user = await hiTradeBLL.LoginUser(request, ct);
                if (user != null)
                {
                    user = tokenBLL.GenerateJwtToken(user);
                }
                else
                {
                    return new BaseResult<UserDTO>
                    {
                        Model = null,
                        ResultType = ResultType.Fail,
                        Message = "Invalid email or password"
                    };
                }
                return new BaseResult<UserDTO>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while logging in");
                return new BaseResult<UserDTO>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
