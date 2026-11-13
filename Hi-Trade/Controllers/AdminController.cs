using Hi_Trade.Models.Common;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hi_Trade.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(IAdminService adminService) : Controller
    {
        [HttpGet("dashboard-stats")]
        public async Task<BaseResult<AdminDashboardStatsDTO>> GetDashboardStats(CancellationToken ct)
        {
            return await adminService.GetDashboardStats(ct);
        }

        [HttpGet("users")]
        public async Task<BaseResult<List<AdminUserDTO>>> GetUsers([FromQuery] string? search, [FromQuery] string? role, [FromQuery] bool? enabled, CancellationToken ct)
        {
            return await adminService.GetUsers(search, role, enabled, ct);
        }

        [HttpPost("users/{id:int}/toggle")]
        public async Task<BaseResult<SaveResponse>> ToggleUserStatus(int id, [FromHeader(Name = "Authorization")] string token, CancellationToken ct)
        {
            return await adminService.ToggleUserStatus(id, token, ct);
        }

        [HttpGet("transactions")]
        public async Task<BaseResult<List<AdminTransactionDTO>>> GetTransactions([FromQuery] string? search, [FromQuery] string? type, CancellationToken ct)
        {
            return await adminService.GetTransactions(search, type, ct);
        }
    }
}
