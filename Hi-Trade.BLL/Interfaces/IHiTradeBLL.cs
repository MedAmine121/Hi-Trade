using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.BLL.Interfaces
{
    public interface IHiTradeBLL
    {
        Task<UserDTO> CreateUser(CreateUserRequest request, CancellationToken ct);
    }
}