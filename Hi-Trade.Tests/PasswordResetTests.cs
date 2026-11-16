using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Hi_Trade.BLL.BLL;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Tests
{
    [TestClass]
    public class PasswordResetTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL = null!;
        private Mock<ITokenBLL> _mockTokenBLL = null!;
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IOptions<RedisOptions>> _mockRedisOptions = null!;
        private Mock<ICacheService> _mockCacheService = null!;
        private Mock<IEmailService> _mockEmailService = null!;
        private UserService _userService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHiTradeBLL = new Mock<IHiTradeBLL>();
            _mockTokenBLL = new Mock<ITokenBLL>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockRedisOptions = new Mock<IOptions<RedisOptions>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockEmailService = new Mock<IEmailService>();

            _mockRedisOptions.Setup(o => o.Value).Returns(new RedisOptions());

            _userService = new UserService(
                _mockHiTradeBLL.Object,
                _mockTokenBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object,
                _mockRedisOptions.Object,
                _mockCacheService.Object,
                _mockEmailService.Object
            );
        }

        #region ForgotPassword Tests

        [TestMethod]
        public async Task ForgotPassword_WhenUserExists_GeneratesTokenAndSendsEmail()
        {
            // Arrange
            var email = "forgot@example.com";
            var request = new ForgotPasswordRequest { Email = email };
            var user = new UserDTO { Email = email, FullName = "Forgot User" };

            _mockHiTradeBLL.Setup(b => b.GeneratePasswordResetToken(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("reset-token-123");
            _mockHiTradeBLL.Setup(b => b.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailService.Setup(e => e.SendPasswordResetAsync(email, "Forgot User", "reset-token-123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.ForgotPassword(request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Success.Should().BeTrue();
            result.Model.Message.Should().Contain("password reset instructions have been sent");

            _mockEmailService.Verify(e => e.SendPasswordResetAsync(email, "Forgot User", "reset-token-123", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ForgotPassword_WhenUserDoesNotExist_StillReturnsSuccessForSecurity()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var request = new ForgotPasswordRequest { Email = email };

            _mockHiTradeBLL.Setup(b => b.GeneratePasswordResetToken(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _userService.ForgotPassword(request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Success.Should().BeTrue();
            _mockEmailService.Verify(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ResetPassword Tests

        [TestMethod]
        public async Task ResetPassword_WithValidToken_ResetsPasswordAndEvictsCache()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "reset@example.com",
                Token = "valid-token",
                NewPassword = "NewSecurePassword123!"
            };

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Password has been reset successfully."
            };

            _mockHiTradeBLL.Setup(b => b.ResetPassword(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            // Act
            var result = await _userService.ResetPassword(request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Success.Should().BeTrue();
            result.Model.Message.Should().Be("Password has been reset successfully.");

            _mockCacheService.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains("reset@example.com")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ResetPassword_WithInvalidToken_ReturnsFail()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "reset@example.com",
                Token = "invalid-token",
                NewPassword = "NewSecurePassword123!"
            };

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Invalid or expired reset token"
            };

            _mockHiTradeBLL.Setup(b => b.ResetPassword(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            // Act
            var result = await _userService.ResetPassword(request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Fail);
            result.Model!.Success.Should().BeFalse();
            result.Model.Message.Should().Be("Invalid or expired reset token");
        }

        #endregion

        #region Validator Tests

        [TestMethod]
        public void ResetPasswordValidator_WithValidData_PassesValidation()
        {
            var validator = new ResetPasswordValidator();
            var model = new ResetPasswordRequest
            {
                Email = "valid@example.com",
                Token = "token123",
                NewPassword = "ValidPassword123!"
            };

            var result = validator.Validate(model);

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void ResetPasswordValidator_WithPasswordUnder12Chars_FailsValidation()
        {
            var validator = new ResetPasswordValidator();
            var model = new ResetPasswordRequest
            {
                Email = "valid@example.com",
                Token = "token123",
                NewPassword = "Short1!"
            };

            var result = validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("at least 12 characters"));
        }

        [TestMethod]
        public void ForgotPasswordValidator_WithEmptyEmail_FailsValidation()
        {
            var validator = new ForgotPasswordValidator();
            var model = new ForgotPasswordRequest { Email = "" };

            var result = validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        #endregion

        #region BLL Unit Tests

        [TestMethod]
        public async Task HiTradeBLL_GeneratePasswordResetToken_WhenUserExists_ReturnsToken()
        {
            var mockDal = new Mock<IHiTradeDAL>();
            var mockConfig = new Mock<IConfiguration>();
            var bll = new HiTradeBLL(mockDal.Object, mockConfig.Object);

            mockDal.Setup(d => d.GetUserByEmail("existing@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Email = "existing@example.com" });
            mockDal.Setup(d => d.SetPasswordResetToken("existing@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var token = await bll.GeneratePasswordResetToken("existing@example.com", CancellationToken.None);

            token.Should().NotBeNullOrWhiteSpace();
            mockDal.Verify(d => d.SetPasswordResetToken("existing@example.com", token!, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HiTradeBLL_GeneratePasswordResetToken_WhenUserNotFound_ReturnsNull()
        {
            var mockDal = new Mock<IHiTradeDAL>();
            var mockConfig = new Mock<IConfiguration>();
            var bll = new HiTradeBLL(mockDal.Object, mockConfig.Object);

            mockDal.Setup(d => d.GetUserByEmail("missing@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null!);

            var token = await bll.GeneratePasswordResetToken("missing@example.com", CancellationToken.None);

            token.Should().BeNull();
            mockDal.Verify(d => d.SetPasswordResetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HiTradeBLL_ResetPassword_HashesPasswordAndCallsDAL()
        {
            var mockDal = new Mock<IHiTradeDAL>();
            var mockConfig = new Mock<IConfiguration>();
            var bll = new HiTradeBLL(mockDal.Object, mockConfig.Object);

            string? capturedHash = null;
            mockDal.Setup(d => d.ResetPassword("user@example.com", "token123", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, CancellationToken>((_, _, hash, _) => capturedHash = hash)
                .ReturnsAsync((true, "Password has been reset successfully."));

            var request = new ResetPasswordRequest
            {
                Email = "user@example.com",
                Token = "token123",
                NewPassword = "MySecurePassword2026!"
            };

            var result = await bll.ResetPassword(request, CancellationToken.None);

            result.Success.Should().BeTrue();
            capturedHash.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify("MySecurePassword2026!", capturedHash!).Should().BeTrue();
        }

        #endregion
    }
}
