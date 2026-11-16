namespace Hi_Trade.Models.Common
{
    public class EmailOptions
    {
        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 1025;
        public string SenderEmail { get; set; } = "no-reply@hi-trade.com";
        public string SenderName { get; set; } = "Hi-Trade";
    }
}
