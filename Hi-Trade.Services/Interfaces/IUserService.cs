using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;

namespace Hi_Trade.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResult<UserDTO>> CreateUser(CreateUserRequest request, CancellationToken ct);
        Task<BaseResult<UserDTO>> LoginUser(LoginUserRequest request, CancellationToken ct);
        Task<bool> CheckBlacklisted(string token);
        Task<BaseResult<SaveResponse>> LogoutUser(string token);
        Task<BaseResult<UserDTO>> FetchUser(string token, CancellationToken ct);
        Task<BaseResult<SaveResponse>> GetCheckoutLink(string token, AddFundsRequest request, CancellationToken ct);
        Task<BaseResult<SaveResponse>> ConfirmPayment(string token, ConfirmPaymentRequest request, CancellationToken ct);
    }
}