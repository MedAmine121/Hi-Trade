using FluentValidation;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResultType = Hi_Trade.Models.Common.ResultType;

namespace Hi_Trade.Services.Services
{
    public class UserService(IHiTradeBLL hiTradeBLL, ITokenBLL tokenBLL, ILogger<UserService> logger, IServiceProvider serviceProvider, IOptions<RedisOptions> redisOptions) : BaseService(serviceProvider), IUserService
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
        public async Task<BaseResult<SaveResponse>> LogoutUser(string token)
        {
            try
            {
                token = token.Replace("Bearer ", "");
                var redis = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { { redisOptions.Value.EndpointUrl, redisOptions.Value.Port } },
                        User = redisOptions.Value.User,
                        Password = redisOptions.Value.Password
                    }
                );
                IDatabase db = redis.GetDatabase();
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if(jti == null || expClaim == null)
                {
                    return new BaseResult<SaveResponse>
                    {
                        Model = new SaveResponse { Success = false, Message = "Invalid token" },
                        ResultType = ResultType.BadRequest
                    };
                }
                var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
                await db.StringSetAsync(jti, "revoked", exp - DateTime.UtcNow);
                return new BaseResult<SaveResponse>
                {
                    Model = new SaveResponse { Success = true },
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while logging out");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<bool> CheckBlacklisted(string token)
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { { redisOptions.Value.EndpointUrl, redisOptions.Value.Port } },
                        User = redisOptions.Value.User,
                        Password = redisOptions.Value.Password
                    }
                );
                IDatabase db = redis.GetDatabase();
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                var blacklisted = await db.StringGetAsync(jti);
                if(blacklisted.HasValue)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking if the token is blacklisted");
                return false;
            }
        }
    }
}
