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
    public class EmailVerificationTests
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

        #region VerifyEmail Tests

        [TestMethod]
        public async Task VerifyEmail_WithValidCode_ReturnsSuccessAndEvictsCache()
        {
            // Arrange
            var request = new VerifyEmailRequest
            {
                Email = "trader@example.com",
                Code = "123456"
            };

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Email verified successfully"
            };

            _mockHiTradeBLL.Setup(b => b.VerifyEmail(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            // Act
            var result = await _userService.VerifyEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model!.Success.Should().BeTrue();
            result.Model.Message.Should().Be("Email verified successfully");

            _mockCacheService.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains("trader@example.com")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task VerifyEmail_WithInvalidCode_ReturnsFail()
        {
            // Arrange
            var request = new VerifyEmailRequest
            {
                Email = "trader@example.com",
                Code = "000000"
            };

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Invalid verification code"
            };

            _mockHiTradeBLL.Setup(b => b.VerifyEmail(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            // Act
            var result = await _userService.VerifyEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Fail);
            result.Model.Should().NotBeNull();
            result.Model!.Success.Should().BeFalse();
            result.Model.Message.Should().Be("Invalid verification code");
        }

        [TestMethod]
        public async Task VerifyEmail_WhenValidatorFails_ReturnsBadRequest()
        {
            // Arrange
            var request = new VerifyEmailRequest { Email = "", Code = "" };
            var mockValidator = new Mock<IValidator<VerifyEmailRequest>>();
            mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Email is required.") }));

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IValidator<VerifyEmailRequest>)))
                .Returns(mockValidator.Object);

            // Act
            var result = await _userService.VerifyEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.BadRequest);
            result.Model.Should().BeNull();
        }

        #endregion

        #region ResendVerificationEmail Tests

        [TestMethod]
        public async Task ResendVerificationEmail_WhenUserExistsAndUnverified_SendsEmailAndReturnsSuccess()
        {
            // Arrange
            var email = "unverified@example.com";
            var request = new ResendVerificationEmailRequest { Email = email };
            var user = new UserDTO { Email = email, FullName = "Trader Joe", IsEmailVerified = false };

            _mockHiTradeBLL.Setup(b => b.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockHiTradeBLL.Setup(b => b.GenerateEmailVerificationCode(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("654321");
            _mockEmailService.Setup(e => e.SendEmailVerificationAsync(email, "Trader Joe", "654321", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.ResendVerificationEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Success.Should().BeTrue();
            _mockEmailService.Verify(e => e.SendEmailVerificationAsync(email, "Trader Joe", "654321", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ResendVerificationEmail_WhenUserAlreadyVerified_ReturnsSuccessWithoutResending()
        {
            // Arrange
            var email = "verified@example.com";
            var request = new ResendVerificationEmailRequest { Email = email };
            var user = new UserDTO { Email = email, FullName = "Trader Jane", IsEmailVerified = true };

            _mockHiTradeBLL.Setup(b => b.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ResendVerificationEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Message.Should().Contain("already verified");
            _mockEmailService.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task ResendVerificationEmail_WhenUserNotFound_ReturnsFail()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var request = new ResendVerificationEmailRequest { Email = email };

            _mockHiTradeBLL.Setup(b => b.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO)null!);

            // Act
            var result = await _userService.ResendVerificationEmail(null, request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Fail);
            result.Model!.Success.Should().BeFalse();
        }

        #endregion

        #region Validator Unit Tests

        [TestMethod]
        public void VerifyEmailValidator_WithValidData_PassesValidation()
        {
            var validator = new VerifyEmailValidator();
            var model = new VerifyEmailRequest { Email = "test@domain.com", Code = "123456" };

            var result = validator.Validate(model);

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void VerifyEmailValidator_WithEmptyCode_FailsValidation()
        {
            var validator = new VerifyEmailValidator();
            var model = new VerifyEmailRequest { Email = "test@domain.com", Code = "" };

            var result = validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Code");
        }

        [TestMethod]
        public void ResendVerificationEmailValidator_WithInvalidEmail_FailsValidation()
        {
            var validator = new ResendVerificationEmailValidator();
            var model = new ResendVerificationEmailRequest { Email = "not-an-email" };

            var result = validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        #endregion

        #region BLL Unit Tests

        [TestMethod]
        public async Task HiTradeBLL_VerifyEmail_DelegatesToDAL()
        {
            var mockDal = new Mock<IHiTradeDAL>();
            var mockConfig = new Mock<IConfiguration>();
            var bll = new HiTradeBLL(mockDal.Object, mockConfig.Object);

            mockDal.Setup(d => d.VerifyEmail("user@example.com", "123456", It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "Email verified successfully"));

            var request = new VerifyEmailRequest { Email = "user@example.com", Code = "123456" };
            var result = await bll.VerifyEmail(request, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Message.Should().Be("Email verified successfully");
        }

        [TestMethod]
        public async Task HiTradeBLL_GenerateEmailVerificationCode_CallsDALAndReturns6DigitCode()
        {
            var mockDal = new Mock<IHiTradeDAL>();
            var mockConfig = new Mock<IConfiguration>();
            var bll = new HiTradeBLL(mockDal.Object, mockConfig.Object);

            mockDal.Setup(d => d.SetEmailVerificationCode("user@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var code = await bll.GenerateEmailVerificationCode("user@example.com", CancellationToken.None);

            code.Should().HaveLength(6);
            int.TryParse(code, out _).Should().BeTrue();
            mockDal.Verify(d => d.SetEmailVerificationCode("user@example.com", code, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
