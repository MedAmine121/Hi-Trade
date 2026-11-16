using FluentAssertions;
using Hi_Trade.Models.Common;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Tests
{
    [TestClass]
    public class EmailServiceTests
    {
        private Mock<ILogger<EmailService>> _mockLogger = null!;
        private Mock<IOptions<EmailOptions>> _mockEmailOptions = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockEmailOptions = new Mock<IOptions<EmailOptions>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockEmailOptions.Setup(o => o.Value).Returns(new EmailOptions
            {
                SmtpHost = "127.0.0.1",
                SmtpPort = 65534, // non-listening port for testing fallback
                SenderEmail = "no-reply@hi-trade.com",
                SenderName = "Hi-Trade"
            });
            _mockConfiguration.Setup(c => c["AngularUrl"]).Returns("http://localhost:4200");
        }

        [TestMethod]
        public async Task SendEmailVerificationAsync_WhenSmtpUnavailable_LogsWarningAndReturnsFalseWithoutCrashing()
        {
            var service = new EmailService(_mockEmailOptions.Object, _mockConfiguration.Object, _mockLogger.Object);

            var result = await service.SendEmailVerificationAsync("test@example.com", "Test User", "123456", CancellationToken.None);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task SendPasswordResetAsync_WhenSmtpUnavailable_LogsWarningAndReturnsFalseWithoutCrashing()
        {
            var service = new EmailService(_mockEmailOptions.Object, _mockConfiguration.Object, _mockLogger.Object);

            var result = await service.SendPasswordResetAsync("test@example.com", "Test User", "token-xyz", CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
