using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerificationAsync(string toEmail, string fullName, string code, CancellationToken ct = default);
        Task<bool> SendPasswordResetAsync(string toEmail, string fullName, string token, CancellationToken ct = default);
    }
}
