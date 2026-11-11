namespace Hi_Trade.Models.Common
{
    public class AdminSeedOptions
    {
        public string Email { get; set; } = "admin@hi-trade.com";
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = "System Admin";
        public string Address { get; set; } = "Hi-Trade HQ";
    }
}
