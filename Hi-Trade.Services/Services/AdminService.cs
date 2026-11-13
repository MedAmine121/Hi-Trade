using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class AdminService(IHiTradeBLL hiTradeBLL, ILogger<AdminService> logger, IServiceProvider serviceProvider)
        : BaseService(serviceProvider), IAdminService
    {
        public async Task<BaseResult<AdminDashboardStatsDTO>> GetDashboardStats(CancellationToken ct)
        {
            try
            {
                var stats = await hiTradeBLL.GetAdminDashboardStats(ct);
                return new BaseResult<AdminDashboardStatsDTO>
                {
                    Model = stats,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching admin dashboard stats");
                return new BaseResult<AdminDashboardStatsDTO>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<List<AdminUserDTO>>> GetUsers(string? search, string? role, bool? enabled, CancellationToken ct)
        {
            try
            {
                var users = await hiTradeBLL.GetAdminUsers(search, role, enabled, ct);
                return new BaseResult<List<AdminUserDTO>>
                {
                    Model = users,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching admin users");
                return new BaseResult<List<AdminUserDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<SaveResponse>> ToggleUserStatus(int userId, string token, CancellationToken ct)
        {
            try
            {
                string adminEmail = GetUserEmailFromToken(token);
                var response = await hiTradeBLL.ToggleUserStatus(userId, adminEmail, ct);
                return new BaseResult<SaveResponse>
                {
                    Model = response,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while toggling user status for ID {UserId}", userId);
                return new BaseResult<SaveResponse>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResult<List<AdminTransactionDTO>>> GetTransactions(string? search, string? type, CancellationToken ct)
        {
            try
            {
                var transactions = await hiTradeBLL.GetAdminTransactions(search, type, ct);
                return new BaseResult<List<AdminTransactionDTO>>
                {
                    Model = transactions,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching admin transactions");
                return new BaseResult<List<AdminTransactionDTO>>
                {
                    Model = null,
                    ResultType = ResultType.Error,
                    Message = ex.Message
                };
            }
        }
    }
}
