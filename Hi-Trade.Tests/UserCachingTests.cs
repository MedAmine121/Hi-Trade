using FluentAssertions;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Common;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class UserCachingTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL = null!;
        private Mock<ITokenBLL> _mockTokenBLL = null!;
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IOptions<RedisOptions>> _mockRedisOptions = null!;
        private Mock<ICacheService> _mockCacheService = null!;
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

            _mockRedisOptions.Setup(o => o.Value).Returns(new RedisOptions());

            _userService = new UserService(
                _mockHiTradeBLL.Object,
                _mockTokenBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object,
                _mockRedisOptions.Object,
                _mockCacheService.Object
            );
        }

        private static string GenerateJwt(string email, string jti, DateTime expires)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("a-very-long-and-secure-test-key-32-chars!!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, email),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return "Bearer " + tokenHandler.WriteToken(token);
        }

        [TestMethod]
        public async Task FetchUser_WhenCached_ReturnsCachedUser_WithoutCallingBLL()
        {
            // Arrange
            var email = "cached@example.com";
            var token = GenerateJwt(email, Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
            var cachedUser = new UserDTO
            {
                Id = 10,
                Email = email,
                FullName = "Cached User",
                Balance = 1500m
            };

            _mockCacheService.Setup(c => c.GetAsync<UserDTO>(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedUser);

            // Act
            var result = await _userService.FetchUser(token, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(cachedUser);

            _mockHiTradeBLL.Verify(b => b.FetchUser(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task FetchUser_WhenNotCached_CallsBLL_AndCachesResult()
        {
            // Arrange
            var email = "uncached@example.com";
            var token = GenerateJwt(email, Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
            var bllUser = new UserDTO
            {
                Id = 11,
                Email = email,
                FullName = "Fresh User",
                Balance = 2500m
            };

            _mockCacheService.Setup(c => c.GetAsync<UserDTO>(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            _mockHiTradeBLL.Setup(b => b.FetchUser(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bllUser);

            // Act
            var result = await _userService.FetchUser(token, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(bllUser);

            _mockHiTradeBLL.Verify(b => b.FetchUser(email, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.UserProfile(email),
                bllUser,
                It.Is<TimeSpan?>(t => t == CacheKeys.DefaultUserExpiry),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task LoginUser_WhenSuccessful_CachesUser()
        {
            // Arrange
            var loginRequest = new LoginUserRequest
            {
                Email = "login@example.com",
                Password = "Password123!"
            };
            var loggedUser = new UserDTO
            {
                Id = 12,
                Email = loginRequest.Email,
                FullName = "Logged In User"
            };

            _mockHiTradeBLL.Setup(b => b.LoginUser(loginRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(loggedUser);
            _mockTokenBLL.Setup(t => t.GenerateJwtToken(loggedUser))
                .Returns(loggedUser);

            // Act
            var result = await _userService.LoginUser(loginRequest, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.UserProfile(loginRequest.Email),
                loggedUser,
                It.Is<TimeSpan?>(t => t == CacheKeys.DefaultUserExpiry),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task EditProfile_WhenSuccessful_InvalidatesOldCache_AndCachesUpdatedUser()
        {
            // Arrange
            var oldEmail = "old@example.com";
            var newEmail = "new@example.com";
            var token = GenerateJwt(oldEmail, Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));

            var editRequest = new EditProfileRequest
            {
                Email = newEmail,
                FullName = "Updated Name",
                Address = "New Address"
            };

            var updatedUser = new UserDTO
            {
                Id = 15,
                Email = newEmail,
                FullName = "Updated Name",
                Address = "New Address"
            };

            _mockHiTradeBLL.Setup(b => b.EditProfile(It.IsAny<EditProfileRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedUser);
            _mockTokenBLL.Setup(t => t.GenerateJwtToken(updatedUser))
                .Returns(updatedUser);

            // Act
            var result = await _userService.EditProfile(token, editRequest, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserProfile(oldEmail), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserPortfolios(oldEmail), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.UserProfile(newEmail),
                updatedUser,
                It.Is<TimeSpan?>(t => t == CacheKeys.DefaultUserExpiry),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmPayment_WhenSuccessful_InvalidatesUserCache()
        {
            // Arrange
            var email = "payer@example.com";
            var token = GenerateJwt(email, Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
            var confirmRequest = new ConfirmPaymentRequest
            {
                PaymentId = "pi_12345"
            };

            _mockHiTradeBLL.Setup(b => b.ConfirmPayment(It.IsAny<ConfirmPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "Payment verified" });

            // Act
            var result = await _userService.ConfirmPayment(token, confirmRequest, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task LogoutUser_BlacklistsToken_AndInvalidatesUserCache()
        {
            // Arrange
            var email = "logout@example.com";
            var jti = "jti-logout-123";
            var expires = DateTime.UtcNow.AddHours(1);
            var token = GenerateJwt(email, jti, expires);

            // Act
            var result = await _userService.LogoutUser(token);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.TokenBlacklist(jti),
                "revoked",
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserPortfolios(email), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task CheckBlacklisted_WhenBlacklistedInCache_ReturnsTrue()
        {
            // Arrange
            var jti = "revoked-token-jti";
            var token = GenerateJwt("user@example.com", jti, DateTime.UtcNow.AddHours(1));

            _mockCacheService.Setup(c => c.ExistsAsync(CacheKeys.TokenBlacklist(jti), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var isBlacklisted = await _userService.CheckBlacklisted(token);

            // Assert
            isBlacklisted.Should().BeTrue();
        }

        [TestMethod]
        public async Task CheckBlacklisted_WhenNotBlacklistedInCache_ReturnsFalse()
        {
            // Arrange
            var jti = "active-token-jti";
            var token = GenerateJwt("user@example.com", jti, DateTime.UtcNow.AddHours(1));

            _mockCacheService.Setup(c => c.ExistsAsync(CacheKeys.TokenBlacklist(jti), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var isBlacklisted = await _userService.CheckBlacklisted(token);

            // Assert
            isBlacklisted.Should().BeFalse();
        }
    }
}
