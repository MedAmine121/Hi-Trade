using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public List<Portfolio> Portfolios { get; set; } = new();
        public Roles Role { get; set; } = Roles.User;
        public List<PaymentTrace> payments { get; set; } = [];
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiresAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }
    }
}
