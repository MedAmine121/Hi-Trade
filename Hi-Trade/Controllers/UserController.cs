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
        [HttpPost("create")]
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
    }
}
