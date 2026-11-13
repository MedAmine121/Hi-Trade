using Hi_Trade.Models.Common;
using System;

namespace Hi_Trade.Models.Responses
{
    public class AdminUserDTO : BaseDTO
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Role { get; set; } = string.Empty;
        public int PortfoliosCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Enabled { get; set; }
    }
}
