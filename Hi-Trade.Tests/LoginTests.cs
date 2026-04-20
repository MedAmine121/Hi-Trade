using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class LoginTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL;
        private Mock<ITokenBLL> _mockTokenBLL;
        private Mock<ILogger<UserService>> _mockLogger;
        private Mock<IServiceProvider> _mockServiceProvider;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockHiTradeBLL = new Mock<IHiTradeBLL>();
            _mockTokenBLL = new Mock<ITokenBLL>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _userService = new UserService(
                _mockHiTradeBLL.Object,
                _mockTokenBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object
            );
        }

        #region LoginUser Success Cases

        [TestMethod]
        public async Task LoginUser_WithValidCredentials_ReturnsSuccessResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "ValidPassword123"
            };

            var userFromBll = new UserDTO
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                ProfilePictureUrl = "https://example.com/pic.jpg",
                Balance = 1000,
                Role = Roles.User
            };

            var userWithToken = new UserDTO
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                ProfilePictureUrl = "https://example.com/pic.jpg",
                Balance = 1000,
                Role = Roles.User,
                Token = "valid.jwt.token"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFromBll);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userFromBll))
                .Returns(userWithToken);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Id.Should().Be(1);
            result.Model.Email.Should().Be("test@example.com");
            result.Model.Token.Should().Be("valid.jwt.token");
            result.Message.Should().BeNull();

            _mockHiTradeBLL.Verify(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()), Times.Once);
            _mockTokenBLL.Verify(x => x.GenerateJwtToken(userFromBll), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WithValidCredentials_GeneratesJwtToken()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };

            var userFromBll = new UserDTO
            {
                Id = 2,
                Email = "admin@example.com",
                FullName = "Admin User",
                Address = "456 Admin Ave",
                ProfilePictureUrl = "https://example.com/admin.jpg",
                Balance = 5000,
                Role = Roles.Admin
            };

            var userWithToken = new UserDTO
            {
                Id = 2,
                Email = "admin@example.com",
                FullName = "Admin User",
                Address = "456 Admin Ave",
                ProfilePictureUrl = "https://example.com/admin.jpg",
                Balance = 5000,
                Role = Roles.Admin,
                Token = "admin.jwt.token"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFromBll);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userFromBll))
                .Returns(userWithToken);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Model.Token.Should().NotBeNullOrEmpty();
            result.Model.Role.Should().Be(Roles.Admin);
            _mockTokenBLL.Verify(x => x.GenerateJwtToken(It.IsAny<UserDTO>()), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_PreservesUserDataInResponse()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "user@example.com",
                Password = "Password123"
            };

            var expectedUser = new UserDTO
            {
                Id = 99,
                Email = "user@example.com",
                FullName = "Full Name",
                Address = "789 Test Road",
                ProfilePictureUrl = "https://example.com/user.jpg",
                Balance = 2500.50m,
                Role = Roles.User
            };

            var userWithToken = new UserDTO
            {
                Id = 99,
                Email = "user@example.com",
                FullName = "Full Name",
                Address = "789 Test Road",
                ProfilePictureUrl = "https://example.com/user.jpg",
                Balance = 2500.50m,
                Role = Roles.User,
                Token = "token"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(expectedUser))
                .Returns(userWithToken);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Model.Should().NotBeNull();
            result.Model.Id.Should().Be(99);
            result.Model.Email.Should().Be("user@example.com");
            result.Model.FullName.Should().Be("Full Name");
            result.Model.Address.Should().Be("789 Test Road");
            result.Model.ProfilePictureUrl.Should().Be("https://example.com/user.jpg");
            result.Model.Balance.Should().Be(2500.50m);
        }

        #endregion

        #region LoginUser Failure Cases

        [TestMethod]
        public async Task LoginUser_WithInvalidEmail_ReturnsFailResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "nonexistent@example.com",
                Password = "SomePassword123"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Fail);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Invalid email or password");

            _mockTokenBLL.Verify(x => x.GenerateJwtToken(It.IsAny<UserDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task LoginUser_WithIncorrectPassword_ReturnsFailResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Fail);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Invalid email or password");

            _mockTokenBLL.Verify(x => x.GenerateJwtToken(It.IsAny<UserDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task LoginUser_WithNullResponse_DoesNotGenerateToken()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "fake@example.com",
                Password = "FakePassword"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            _mockTokenBLL.Verify(x => x.GenerateJwtToken(It.IsAny<UserDTO>()), Times.Never);
            result.ResultType.Should().Be(ResultType.Fail);
        }

        #endregion

        #region LoginUser Exception Cases

        [TestMethod]
        public async Task LoginUser_WithBllException_ReturnsErrorResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WithTokenGenerationException_ReturnsErrorResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var userFromBll = new UserDTO
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                ProfilePictureUrl = "https://example.com/pic.jpg",
                Balance = 1000,
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFromBll);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userFromBll))
                .Throws(new Exception("JWT configuration error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Be("JWT configuration error");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WithOperationCanceledException_ReturnsErrorResult()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _userService.LoginUser(loginRequest, cts.Token);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Error);
        }

        #endregion

        #region LoginUser Edge Cases

        [TestMethod]
        public async Task LoginUser_PassesCorrectRequestToBll()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "specific@example.com",
                Password = "SpecificPassword"
            };

            var userFromBll = new UserDTO
            {
                Id = 1,
                Email = "specific@example.com",
                FullName = "Test",
                Address = "Test St",
                Role = Roles.User
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(It.IsAny<LoginUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFromBll);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(It.IsAny<UserDTO>()))
                .Returns(userFromBll);

            var ct = CancellationToken.None;

            // Act
            await _userService.LoginUser(loginRequest, ct);

            // Assert
            _mockHiTradeBLL.Verify(
                x => x.LoginUser(
                    It.Is<LoginUserRequest>(r => r.Email == "specific@example.com" && r.Password == "SpecificPassword"),
                    ct),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WithEmptyEmail_PassesToBll()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "",
                Password = "Password"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Fail);
            _mockHiTradeBLL.Verify(x => x.LoginUser(loginRequest, ct), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WithWhitespacePassword_PassesToBll()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "   "
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var ct = CancellationToken.None;

            // Act
            var result = await _userService.LoginUser(loginRequest, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Fail);
            _mockHiTradeBLL.Verify(x => x.LoginUser(loginRequest, ct), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_LogsErrorOnException()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var exceptionMessage = "Test error message";
            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var ct = CancellationToken.None;

            // Act
            await _userService.LoginUser(loginRequest, ct);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("logging in")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_DoesNotLogOnSuccess()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var userFromBll = new UserDTO
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                Balance = 1000,
                Role = Roles.User
            };

            var userWithToken = new UserDTO
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                Balance = 1000,
                Role = Roles.User,
                Token = "valid.jwt.token"
            };

            _mockHiTradeBLL.Setup(x => x.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFromBll);

            _mockTokenBLL.Setup(x => x.GenerateJwtToken(userFromBll))
                .Returns(userWithToken);

            var ct = CancellationToken.None;

            // Act
            await _userService.LoginUser(loginRequest, ct);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        #endregion
    }
}
