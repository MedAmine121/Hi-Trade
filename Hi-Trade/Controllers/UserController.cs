using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hi_Trade.Controllers
{
    [Route("user")]
    public class UserController(IUserService userService) : Controller
    {
        [HttpPost("CreateUser")]
        public async Task<BaseResult<UserDTO>> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            return await userService.CreateUser(request, ct);
        }

        [HttpPost("LoginUser")]
        public async Task<BaseResult<UserDTO>> LoginUser([FromBody] LoginUserRequest request, CancellationToken ct)
        {
            return await userService.LoginUser(request, ct);
        }
    }
}
