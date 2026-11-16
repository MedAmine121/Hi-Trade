using FluentValidation;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Common;
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
    public class UserService(IHiTradeBLL hiTradeBLL, ITokenBLL tokenBLL, ILogger<UserService> logger, IServiceProvider serviceProvider, IOptions<RedisOptions> redisOptions, ICacheService? cacheService = null, IEmailService? emailService = null) : BaseService(serviceProvider), IUserService
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
                    if (cacheService != null)
                    {
                        await cacheService.SetAsync(CacheKeys.UserProfile(user.Email), user, CacheKeys.DefaultUserExpiry, ct);
                    }
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
                token = token.Replace("Bearer ", "").Trim();
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
                var ttl = exp - DateTime.UtcNow;
                if (ttl <= TimeSpan.Zero)
                {
                    ttl = TimeSpan.FromMinutes(1);
                }

                if (cacheService != null)
                {
                    await cacheService.SetAsync(CacheKeys.TokenBlacklist(jti), "revoked", ttl);
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    if (!string.IsNullOrWhiteSpace(emailClaim))
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserProfile(emailClaim));
                        await cacheService.RemoveAsync(CacheKeys.UserPortfolios(emailClaim));
                    }
                }
                else
                {
                    var redis = await ConnectionMultiplexer.ConnectAsync(
                        new ConfigurationOptions
                        {
                            EndPoints = { { redisOptions.Value.EndpointUrl, redisOptions.Value.Port } },
                            User = redisOptions.Value.User,
                            Password = redisOptions.Value.Password
                        }
                    );
                    IDatabase db = redis.GetDatabase();
                    await db.StringSetAsync(jti, "revoked", ttl);
                }

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
        public async Task<BaseResult<UserDTO>> FetchUser(string token, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                if (cacheService != null)
                {
                    var cachedUser = await cacheService.GetAsync<UserDTO>(CacheKeys.UserProfile(email), ct);
                    if (cachedUser != null)
                    {
                        return new BaseResult<UserDTO>
                        {
                            Model = cachedUser,
                            ResultType = ResultType.Success
                        };
                    }
                }

                UserDTO? user = await hiTradeBLL.FetchUser(email, ct);
                if (user != null && cacheService != null)
                {
                    await cacheService.SetAsync(CacheKeys.UserProfile(email), user, CacheKeys.DefaultUserExpiry, ct);
                }

                return new BaseResult<UserDTO>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the user");
                return new BaseResult<UserDTO>
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
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", "").Trim());
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                {
                    return false;
                }

                if (cacheService != null)
                {
                    return await cacheService.ExistsAsync(CacheKeys.TokenBlacklist(jti));
                }

                var redis = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { { redisOptions.Value.EndpointUrl, redisOptions.Value.Port } },
                        User = redisOptions.Value.User,
                        Password = redisOptions.Value.Password
                    }
                );
                IDatabase db = redis.GetDatabase();
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
        public async Task<BaseResult<SaveResponse>> GetCheckoutLink(string token, AddFundsRequest request, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                SaveResponse? user = await Validate(hiTradeBLL.GetCheckoutLink!, request, ct);
                return new BaseResult<SaveResponse>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while creating payment intent");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the user");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<SaveResponse>> ConfirmPayment(string token, ConfirmPaymentRequest request, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.Email = email;
                SaveResponse? user = await Validate(hiTradeBLL.ConfirmPayment!, request, ct);
                if (user != null && user.Success && cacheService != null)
                {
                    await cacheService.RemoveAsync(CacheKeys.UserProfile(email), ct);
                    await cacheService.RemoveAsync(CacheKeys.AdminDashboardStats, ct);
                }
                return new BaseResult<SaveResponse>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while creating payment intent");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the user");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
        public async Task<BaseResult<UserDTO>> EditProfile(string token, EditProfileRequest request, CancellationToken ct)
        {
            try
            {
                string email = GetUserEmailFromToken(token);
                request.OldEmail = email;
                UserDTO? user = await Validate(hiTradeBLL.EditProfile!, request, ct);
                if (user != null)
                {
                    user = tokenBLL.GenerateJwtToken(user);
                    if (cacheService != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.OldEmail))
                        {
                            await cacheService.RemoveAsync(CacheKeys.UserProfile(request.OldEmail), ct);
                            await cacheService.RemoveAsync(CacheKeys.UserPortfolios(request.OldEmail), ct);
                        }
                        await cacheService.SetAsync(CacheKeys.UserProfile(user.Email), user, CacheKeys.DefaultUserExpiry, ct);
                    }
                }
                return new BaseResult<UserDTO>
                {
                    Model = user,
                    ResultType = ResultType.Success
                };
            }

            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while editing profile");
                return new BaseResult<UserDTO>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while editing profile");
                return new BaseResult<UserDTO>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<SaveResponse>> VerifyEmail(string? token, VerifyEmailRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(token))
                {
                    request.Email = GetUserEmailFromToken(token);
                }

                SaveResponse? result = await Validate(hiTradeBLL.VerifyEmail!, request, ct);
                if (result != null)
                {
                    if (result.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserProfile(request.Email), ct);
                        await cacheService.RemoveAsync(CacheKeys.AdminDashboardStats, ct);
                    }
                    return new BaseResult<SaveResponse>
                    {
                        Model = result,
                        ResultType = result.Success ? ResultType.Success : ResultType.Fail,
                        Message = result.Message
                    };
                }
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = "Verification failed."
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while verifying email");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while verifying email");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<SaveResponse>> ResendVerificationEmail(string? token, ResendVerificationEmailRequest? request, CancellationToken ct)
        {
            try
            {
                string email = string.Empty;
                if (request != null && !string.IsNullOrWhiteSpace(request.Email))
                {
                    email = request.Email;
                }
                else if (!string.IsNullOrWhiteSpace(token))
                {
                    email = GetUserEmailFromToken(token);
                }

                var req = new ResendVerificationEmailRequest { Email = email };
                await Validate<ResendVerificationEmailRequest, SaveResponse>((r, _) => Task.FromResult<SaveResponse?>(new SaveResponse { Success = true }), req, ct);

                var user = await hiTradeBLL.FetchUser(email, ct);
                if (user == null)
                {
                    return new BaseResult<SaveResponse>
                    {
                        Model = new SaveResponse { Success = false, Message = "User not found." },
                        ResultType = ResultType.Fail,
                        Message = "User not found."
                    };
                }
                if (user.IsEmailVerified)
                {
                    return new BaseResult<SaveResponse>
                    {
                        Model = new SaveResponse { Success = true, Message = "Email is already verified." },
                        ResultType = ResultType.Success,
                        Message = "Email is already verified."
                    };
                }

                var code = await hiTradeBLL.GenerateEmailVerificationCode(email, ct);
                if (emailService != null)
                {
                    await emailService.SendEmailVerificationAsync(user.Email, user.FullName, code, ct);
                }

                return new BaseResult<SaveResponse>
                {
                    Model = new SaveResponse { Success = true, Message = "Verification email sent successfully." },
                    ResultType = ResultType.Success,
                    Message = "Verification email sent successfully."
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while resending verification email");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while resending verification email");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<SaveResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
        {
            try
            {
                await Validate<ForgotPasswordRequest, SaveResponse>((r, _) => Task.FromResult<SaveResponse?>(new SaveResponse { Success = true }), request, ct);

                var token = await hiTradeBLL.GeneratePasswordResetToken(request.Email, ct);
                if (!string.IsNullOrWhiteSpace(token) && emailService != null)
                {
                    try
                    {
                        var user = await hiTradeBLL.FetchUser(request.Email, ct);
                        if (user != null)
                        {
                            await emailService.SendPasswordResetAsync(user.Email, user.FullName, token, ct);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to send password reset email to {Email}", request.Email);
                    }
                }

                var response = new SaveResponse
                {
                    Success = true,
                    Message = "If an account with that email exists, password reset instructions have been sent."
                };
                return new BaseResult<SaveResponse>
                {
                    Model = response,
                    ResultType = ResultType.Success,
                    Message = response.Message
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while processing forgot password request");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing forgot password request");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<SaveResponse>> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
        {
            try
            {
                SaveResponse? result = await Validate(hiTradeBLL.ResetPassword!, request, ct);
                if (result != null)
                {
                    if (result.Success && cacheService != null)
                    {
                        await cacheService.RemoveAsync(CacheKeys.UserProfile(request.Email), ct);
                    }
                    return new BaseResult<SaveResponse>
                    {
                        Model = result,
                        ResultType = result.Success ? ResultType.Success : ResultType.Fail,
                        Message = result.Message
                    };
                }
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Fail,
                    Message = "Password reset failed."
                };
            }
            catch (ValidationException vex)
            {
                logger.LogWarning(vex, "Validation failed while resetting password");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.BadRequest,
                    Message = vex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while resetting password");
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
