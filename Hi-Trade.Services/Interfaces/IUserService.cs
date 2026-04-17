using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResult<UserDTO>> CreateUser(CreateUserRequest request, CancellationToken ct);
    }
}