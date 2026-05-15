using FluentAssertions;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class UserServiceTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL;
        private Mock<ITokenBLL> _mockTokenBLL;
        private Mock<ILogger<UserService>> _mockLogger;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IOptions<RedisOptions>> _mockRedisOptions;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockHiTradeBLL = new Mock<IHiTradeBLL>();
            _mockTokenBLL = new Mock<ITokenBLL>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockRedisOptions = new Mock<IOptions<RedisOptions>>();

            _userService = new UserService(
                _mockHiTradeBLL.Object,
                _mockTokenBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object,
                _mockRedisOptions.Object
            );
        }

        #region CreateUser Success Cases

        [TestMethod]
        public async Task CreateUser_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "newuser@example.com",
                Password = "ValidPassword123!",
                FullName = "New User",
                Address = "123 Main Street"
            };

            var userDTO = new UserDTO
            {
                Id = 1,
                Email = "newuser@example.com",
                FullName = "New User",
                Address = "123 Main Street",
                ProfilePictureUrl = "",
                Balance = 0,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.CreateUser(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDTO);

            var ct = CancellationToken.None;
            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userDTO))
                .Returns(userDTO);
            // Act
            var result = await _userService.CreateUser(request, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Email.Should().Be("newuser@example.com");

            _mockHiTradeBLL.Verify(x => x.CreateUser(request, ct), Times.Once);
        }

        [TestMethod]
        public async Task CreateUser_WithValidEmail_CreatesUserSuccessfully()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@domain.co.uk",
                Password = "SecurePassword123!",
                FullName = "Test User",
                Address = "456 Oak Avenue"
            };

            var userDTO = new UserDTO
            {
                Id = 2,
                Email = "test@domain.co.uk",
                FullName = "Test User",
                Address = "456 Oak Avenue",
                ProfilePictureUrl = "",
                Balance = 0,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.CreateUser(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDTO);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userDTO))
                .Returns(userDTO);
            var ct = CancellationToken.None;

            // Act
            var result = await _userService.CreateUser(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Email.Should().Be("test@domain.co.uk");
        }

        #endregion

        #region CreateUser Failure Cases

        [TestMethod]
        public async Task CreateUser_WithDuplicateEmail_ReturnsFailResult()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "existing@example.com",
                Password = "ValidPassword123!",
                FullName = "Existing User",
                Address = "789 Pine Street"
            };

            _mockHiTradeBLL.Setup(x => x.CreateUser(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("User with the same email already exists"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.CreateUser(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Contain("already exists");
        }

        [TestMethod]
        public async Task CreateUser_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                Password = "ValidPassword123!",
                FullName = "Test User",
                Address = "123 Test St"
            };

            _mockHiTradeBLL.Setup(x => x.CreateUser(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.CreateUser(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region FetchUser Success Cases

        [TestMethod]
        public async Task FetchUser_WithValidEmail_ReturnSuccessWithUserData()
        {
            // Arrange
            var email = "existing@example.com";

            var userDTO = new UserDTO
            {
                Id = 1,
                Email = "existing@example.com",
                FullName = "Existing User",
                Address = "123 Main St",
                ProfilePictureUrl = "https://example.com/pic.jpg",
                Balance = 5000.00m,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDTO);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.FetchUser(GenerateJWTTestToken(email), ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Email.Should().Be("existing@example.com");
            result.Model.Balance.Should().Be(5000.00m);

            _mockHiTradeBLL.Verify(x => x.FetchUser(email, ct), Times.Once);
        }

        #endregion

        #region FetchUser Failure Cases

        [TestMethod]
        public async Task FetchUser_WithNonexistentUser_ReturnsErrorResult()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockHiTradeBLL.Setup(x => x.FetchUser(email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("User does not exist"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.FetchUser(GenerateJWTTestToken(email), ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Contain("does not exist");
        }

        #endregion

        #region AddFunds Success Cases

        [TestMethod]
        public async Task AddFunds_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new AddFundsRequest
            {
                Amount = 1000,
                Email = "test@example.com"
            };

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Checkout link generated"
            };

            _mockHiTradeBLL.Setup(x => x.GetCheckoutLink(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.GetCheckoutLink(token, request, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeTrue();

            _mockHiTradeBLL.Verify(x => x.GetCheckoutLink(request, ct), Times.Once);
        }

        #endregion

        #region AddFunds Failure Cases

        [TestMethod]
        public async Task AddFunds_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new AddFundsRequest
            {
                Amount = 500,
                Email = "test@example.com"
            };

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";

            _mockHiTradeBLL.Setup(x => x.GetCheckoutLink(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Payment gateway error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.GetCheckoutLink(token, request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
        }

        #endregion

        #region ConfirmPayment Success Cases

        [TestMethod]
        public async Task ConfirmPayment_WithSucceededPayment_ReturnsSuccessResult()
        {
            // Arrange
            var request = new ConfirmPaymentRequest
            {
                PaymentId = "pi_123456789",
                Email = "test@example.com"
            };

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Funds added successfully"
            };

            _mockHiTradeBLL.Setup(x => x.ConfirmPayment(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.ConfirmPayment(token, request, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeTrue();

            _mockHiTradeBLL.Verify(x => x.ConfirmPayment(request, ct), Times.Once);
        }

        #endregion

        #region ConfirmPayment Failure Cases

        [TestMethod]
        public async Task ConfirmPayment_WithFailedPayment_ReturnsFailResult()
        {
            // Arrange
            var request = new ConfirmPaymentRequest
            {
                PaymentId = "pi_failed",
                Email = "test@example.com"
            };

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Payment not processed yet!"
            };

            _mockHiTradeBLL.Setup(x => x.ConfirmPayment(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.ConfirmPayment(token, request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeFalse();
        }

        #endregion

        #region User Data Integrity

        [TestMethod]
        public async Task CreateUser_PreservesAllUserData()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "integrity@example.com",
                Password = "ValidPassword123!",
                FullName = "Integrity Test",
                Address = "Integrity Avenue"
            };

            var userDTO = new UserDTO
            {
                Id = 999,
                Email = "integrity@example.com",
                FullName = "Integrity Test",
                Address = "Integrity Avenue",
                ProfilePictureUrl = "https://example.com/avatar.jpg",
                Balance = 0,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.CreateUser(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDTO);

            var ct = CancellationToken.None;
            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userDTO))
                .Returns(userDTO);
            // Act
            var result = await _userService.CreateUser(request, ct);

            // Assert
            var user = result.Model;
            user.Id.Should().Be(999);
            user.Email.Should().Be("integrity@example.com");
            user.FullName.Should().Be("Integrity Test");
            user.Address.Should().Be("Integrity Avenue");
            user.Balance.Should().Be(0);
            user.Role.Should().Be(Roles.User);
        }

        [TestMethod]
        public async Task FetchUser_PreservesUserBalance()
        {
            // Arrange
            var email = "balance@example.com";
            
            var userDTO = new UserDTO
            {
                Id = 1,
                Email = "balance@example.com",
                FullName = "Balance User",
                Address = "Balance St",
                ProfilePictureUrl = "",
                Balance = 12345.67m,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDTO);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.FetchUser(GenerateJWTTestToken(email), ct);

            // Assert
            result.Model.Balance.Should().Be(12345.67m);
        }

        #endregion
        public string GenerateJWTTestToken(string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SOME_VERY_SUPER_LONG_TEST_SECRET_KEY"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.User.ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = "Some Issuer",
                Audience = "Some Audience",
                SigningCredentials = credentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
