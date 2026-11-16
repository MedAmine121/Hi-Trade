using Hi_Trade.Models.Common;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class EmailService(IOptions<EmailOptions> emailOptions, IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailOptions _options = emailOptions.Value;
        private readonly string _angularUrl = configuration["AngularUrl"] ?? "http://localhost:4200";

        public async Task<bool> SendEmailVerificationAsync(string toEmail, string fullName, string code, CancellationToken ct = default)
        {
            var verifyUrl = $"{_angularUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(toEmail)}&code={Uri.EscapeDataString(code)}";
            var subject = "Verify your Hi-Trade email address";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f9; margin: 0; padding: 24px; color: #2d3748; }}
        .card {{ max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 36px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); }}
        .header {{ text-align: center; margin-bottom: 24px; }}
        .logo {{ font-size: 32px; font-weight: bold; color: #4f46e5; }}
        .title {{ font-size: 22px; font-weight: 700; color: #1a202c; margin-top: 12px; }}
        .code-box {{ background-color: #f0f4ff; border: 2px dashed #6366f1; border-radius: 8px; padding: 18px; text-align: center; margin: 28px 0; }}
        .code {{ font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #4338ca; }}
        .btn {{ display: inline-block; background-color: #4f46e5; color: #ffffff !important; padding: 12px 28px; border-radius: 6px; font-weight: 600; text-decoration: none; margin-top: 8px; }}
        .footer {{ font-size: 13px; color: #718096; margin-top: 32px; border-top: 1px solid #e2e8f0; padding-top: 16px; text-align: center; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='header'>
            <div class='logo'>📈 Hi-Trade</div>
            <div class='title'>Verify Your Email Address</div>
        </div>
        <p>Hi {fullName},</p>
        <p>Thank you for registering with Hi-Trade! To activate trading, buying, and selling in your portfolios, please enter the verification code below in your account homepage:</p>
        <div class='code-box'>
            <div class='code'>{code}</div>
        </div>
        <p style='text-align: center;'>Or click the button below to verify your email automatically:</p>
        <div style='text-align: center; margin: 20px 0;'>
            <a href='{verifyUrl}' class='btn' target='_blank'>Verify Email Now</a>
        </div>
        <p>This verification code will expire in 24 hours.</p>
        <div class='footer'>
            If you did not create a Hi-Trade account, please disregard this email.<br>&copy; Hi-Trade Trading Platform
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body, ct);
        }

        public async Task<bool> SendPasswordResetAsync(string toEmail, string fullName, string token, CancellationToken ct = default)
        {
            var resetUrl = $"{_angularUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(token)}";
            var subject = "Reset your Hi-Trade password";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f9; margin: 0; padding: 24px; color: #2d3748; }}
        .card {{ max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 36px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); }}
        .header {{ text-align: center; margin-bottom: 24px; }}
        .logo {{ font-size: 32px; font-weight: bold; color: #4f46e5; }}
        .title {{ font-size: 22px; font-weight: 700; color: #1a202c; margin-top: 12px; }}
        .btn {{ display: inline-block; background-color: #4f46e5; color: #ffffff !important; padding: 12px 28px; border-radius: 6px; font-weight: 600; text-decoration: none; margin: 20px 0; }}
        .footer {{ font-size: 13px; color: #718096; margin-top: 32px; border-top: 1px solid #e2e8f0; padding-top: 16px; text-align: center; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='header'>
            <div class='logo'>📈 Hi-Trade</div>
            <div class='title'>Password Reset Request</div>
        </div>
        <p>Hello {fullName},</p>
        <p>We received a request to reset your password for your Hi-Trade account. Click the button below to choose a new password:</p>
        <div style='text-align: center;'>
            <a href='{resetUrl}' class='btn' target='_blank'>Reset Password</a>
        </div>
        <p>Or paste this URL into your browser:</p>
        <p style='word-break: break-all; font-size: 13px; color: #4f46e5;'><a href='{resetUrl}'>{resetUrl}</a></p>
        <p>This password reset link will expire in 1 hour. If you did not request a password reset, you can safely ignore this email.</p>
        <div class='footer'>
            &copy; Hi-Trade Trading Platform
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body, ct);
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
        {
            try
            {
                var host = string.IsNullOrWhiteSpace(_options.SmtpHost) ? "localhost" : _options.SmtpHost;
                var port = _options.SmtpPort <= 0 ? 1025 : _options.SmtpPort;
                var senderEmail = string.IsNullOrWhiteSpace(_options.SenderEmail) ? "no-reply@hi-trade.com" : _options.SenderEmail;
                var senderName = string.IsNullOrWhiteSpace(_options.SenderName) ? "Hi-Trade" : _options.SenderName;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage, ct);
                logger.LogInformation("Email '{Subject}' sent to {ToEmail}", subject, toEmail);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send email '{Subject}' to {ToEmail} via SMTP {Host}:{Port}", subject, toEmail, _options.SmtpHost, _options.SmtpPort);
                return false;
            }
        }
    }
}
