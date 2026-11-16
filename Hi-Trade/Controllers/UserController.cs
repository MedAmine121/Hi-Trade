using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hi_Trade.Controllers
{
    [Route("user")]
    public class UserController(IUserService userService) : Controller
    {
        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<BaseResult<UserDTO>> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            return await userService.CreateUser(request, ct);
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<BaseResult<UserDTO>> LoginUser([FromBody] LoginUserRequest request, CancellationToken ct)
        {
            return await userService.LoginUser(request, ct);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<BaseResult<SaveResponse>> LogoutUser([FromHeader(Name = "Authorization")] string token, CancellationToken ct)
        {
            return await userService.LogoutUser(token);
        }
        [Authorize]
        [HttpGet("fetch")]
        public async Task<BaseResult<UserDTO>> FetchUser([FromHeader(Name = "Authorization")] string token, CancellationToken ct)
        {
            return await userService.FetchUser(token, ct);
        }
        [Authorize(Roles = "User")]
        [HttpPost("checkout")]
        public async Task<BaseResult<SaveResponse>> GetCheckoutLink([FromHeader(Name = "Authorization")] string token, [FromBody] AddFundsRequest request, CancellationToken ct)
        {
            return await userService.GetCheckoutLink(token, request, ct);
        }
        [Authorize(Roles = "User")]
        [HttpPost("confirmpayment")]
        public async Task<BaseResult<SaveResponse>> ConfirmPayment([FromHeader(Name = "Authorization")] string token, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
        {
            return await userService.ConfirmPayment(token, request, ct);
        }
        [Authorize]
        [HttpPost("edit")]
        public async Task<BaseResult<UserDTO>> EditProfile([FromHeader(Name = "Authorization")] string token, [FromBody] EditProfileRequest request, CancellationToken ct)
        {
            return await userService.EditProfile(token, request, ct);
        }
        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<BaseResult<SaveResponse>> VerifyEmail([FromHeader(Name = "Authorization")] string? token, [FromBody] VerifyEmailRequest request, CancellationToken ct)
        {
            return await userService.VerifyEmail(token, request, ct);
        }
        [AllowAnonymous]
        [HttpPost("send-verification")]
        [HttpPost("resend-verification")]
        public async Task<BaseResult<SaveResponse>> ResendVerificationEmail([FromHeader(Name = "Authorization")] string? token, [FromBody] ResendVerificationEmailRequest? request, CancellationToken ct)
        {
            return await userService.ResendVerificationEmail(token, request, ct);
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<BaseResult<SaveResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            return await userService.ForgotPassword(request, ct);
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<BaseResult<SaveResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
        {
            return await userService.ResetPassword(request, ct);
        }
    }
}
