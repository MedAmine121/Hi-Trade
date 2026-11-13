using Hi_Trade.Models.Common;
using Hi_Trade.Models.Responses;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Interfaces
{
    public interface IAdminService
    {
        Task<BaseResult<AdminDashboardStatsDTO>> GetDashboardStats(CancellationToken ct);
        Task<BaseResult<List<AdminUserDTO>>> GetUsers(string? search, string? role, bool? enabled, CancellationToken ct);
        Task<BaseResult<SaveResponse>> ToggleUserStatus(int userId, string token, CancellationToken ct);
        Task<BaseResult<List<AdminTransactionDTO>>> GetTransactions(string? search, string? type, CancellationToken ct);
    }
}
