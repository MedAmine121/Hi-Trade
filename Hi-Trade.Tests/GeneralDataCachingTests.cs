using FluentAssertions;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Common;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class GeneralDataCachingTests
    {
        private Mock<IHiTradeBLL> _mockBLL = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<ICacheService> _mockCacheService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockBLL = new Mock<IHiTradeBLL>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockCacheService = new Mock<ICacheService>();
        }

        private static string GenerateJwt(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("a-very-long-and-secure-test-key-32-chars!!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, email),
                    new Claim(ClaimTypes.Role, "User")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return "Bearer " + tokenHandler.WriteToken(token);
        }

        #region AssetService Tests

        [TestMethod]
        public async Task GetAllEnabledAssets_WhenCached_ReturnsCachedAssets_WithoutCallingBLL()
        {
            // Arrange
            var cachedAssets = new List<AssetDTO>
            {
                new() { Id = 1, Ticker = "AAPL", Name = "Apple", CurrentPrice = 180m, Enabled = true },
                new() { Id = 2, Ticker = "MSFT", Name = "Microsoft", CurrentPrice = 420m, Enabled = true }
            };

            _mockCacheService.Setup(c => c.GetAsync<List<AssetDTO>>(CacheKeys.EnabledAssets, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedAssets);

            var assetLogger = new Mock<ILogger<AssetService>>();
            var assetService = new AssetService(_mockBLL.Object, assetLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await assetService.GetAllEnabledAssets(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(cachedAssets);
            _mockBLL.Verify(b => b.GetAllEnabledAssets(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetAllEnabledAssets_WhenNotCached_CallsBLL_AndCachesResult()
        {
            // Arrange
            var bllAssets = new List<AssetDTO>
            {
                new() { Id = 1, Ticker = "NVDA", Name = "Nvidia", CurrentPrice = 120m, Enabled = true }
            };

            _mockCacheService.Setup(c => c.GetAsync<List<AssetDTO>>(CacheKeys.EnabledAssets, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<AssetDTO>?)null);

            _mockBLL.Setup(b => b.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bllAssets);

            var assetLogger = new Mock<ILogger<AssetService>>();
            var assetService = new AssetService(_mockBLL.Object, assetLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await assetService.GetAllEnabledAssets(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(bllAssets);
            _mockBLL.Verify(b => b.GetAllEnabledAssets(It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.EnabledAssets,
                bllAssets,
                It.Is<TimeSpan?>(t => t == CacheKeys.DefaultAssetsExpiry),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsset_WhenSuccessful_InvalidatesAssetsCache()
        {
            // Arrange
            var request = new CreateAssetRequest { Ticker = "TSLA", Name = "Tesla" };
            _mockBLL.Setup(b => b.CreateAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "Asset created" });

            var assetLogger = new Mock<ILogger<AssetService>>();
            var assetService = new AssetService(_mockBLL.Object, assetLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await assetService.CreateAsset(request, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.EnabledAssets, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPrefixAsync(CacheKeys.AssetsPrefix, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ToggleAssetStatus_WhenSuccessful_InvalidatesAssetsCache()
        {
            // Arrange
            _mockBLL.Setup(b => b.ToggleAssetStatus(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "Status updated" });

            var assetLogger = new Mock<ILogger<AssetService>>();
            var assetService = new AssetService(_mockBLL.Object, assetLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await assetService.ToggleAssetStatus(1, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.EnabledAssets, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPrefixAsync(CacheKeys.AssetsPrefix, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region PortfolioService Tests

        [TestMethod]
        public async Task GetUserPortfolios_WhenCached_ReturnsCachedPortfolios_WithoutCallingBLL()
        {
            // Arrange
            var email = "investor@example.com";
            var token = GenerateJwt(email);
            var cachedPortfolios = new List<PortfolioDTO>
            {
                new() { Id = 1, Name = "Tech Growth", CurrentValue = 10000m }
            };

            _mockCacheService.Setup(c => c.GetAsync<List<PortfolioDTO>>(CacheKeys.UserPortfolios(email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedPortfolios);

            var portfolioLogger = new Mock<ILogger<PortfolioService>>();
            var portfolioService = new PortfolioService(_mockBLL.Object, portfolioLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await portfolioService.GetUserPortfolios(token, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(cachedPortfolios);
            _mockBLL.Verify(b => b.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task BuyAsset_WhenSuccessful_InvalidatesUserAndPortfoliosCache()
        {
            // Arrange
            var email = "trader@example.com";
            var token = GenerateJwt(email);
            var buyRequest = new BuyAssetRequest { AssetId = 1, PortfolioId = 2, Quantity = 5 };

            _mockBLL.Setup(b => b.BuyAsset(buyRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "Purchased" });

            var portfolioLogger = new Mock<ILogger<PortfolioService>>();
            var portfolioService = new PortfolioService(_mockBLL.Object, portfolioLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await portfolioService.BuyAsset(buyRequest, token, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserPortfolios(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SellAsset_WhenSuccessful_InvalidatesUserAndPortfoliosCache()
        {
            // Arrange
            var email = "trader@example.com";
            var token = GenerateJwt(email);
            var sellRequest = new SellAssetRequest { PositionId = 1, PortfolioId = 2, Quantity = 2 };

            _mockBLL.Setup(b => b.SellAsset(sellRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "Sold" });

            var portfolioLogger = new Mock<ILogger<PortfolioService>>();
            var portfolioService = new PortfolioService(_mockBLL.Object, portfolioLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await portfolioService.SellAsset(sellRequest, token, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserProfile(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.UserPortfolios(email), It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region AdminService Tests

        [TestMethod]
        public async Task GetDashboardStats_WhenCached_ReturnsCachedStats_WithoutCallingBLL()
        {
            // Arrange
            var cachedStats = new AdminDashboardStatsDTO
            {
                TotalUsers = 120,
                TotalPortfolios = 80,
                TotalTransactions = 350,
                TotalTradingVolume = 500000m
            };

            _mockCacheService.Setup(c => c.GetAsync<AdminDashboardStatsDTO>(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedStats);

            var adminLogger = new Mock<ILogger<AdminService>>();
            var adminService = new AdminService(_mockBLL.Object, adminLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await adminService.GetDashboardStats(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(cachedStats);
            _mockBLL.Verify(b => b.GetAdminDashboardStats(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetDashboardStats_WhenNotCached_CallsBLL_AndCachesResult()
        {
            // Arrange
            var bllStats = new AdminDashboardStatsDTO
            {
                TotalUsers = 150,
                TotalPortfolios = 100
            };

            _mockCacheService.Setup(c => c.GetAsync<AdminDashboardStatsDTO>(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AdminDashboardStatsDTO?)null);

            _mockBLL.Setup(b => b.GetAdminDashboardStats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bllStats);

            var adminLogger = new Mock<ILogger<AdminService>>();
            var adminService = new AdminService(_mockBLL.Object, adminLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await adminService.GetDashboardStats(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEquivalentTo(bllStats);
            _mockBLL.Verify(b => b.GetAdminDashboardStats(It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                CacheKeys.AdminDashboardStats,
                bllStats,
                It.Is<TimeSpan?>(t => t == CacheKeys.DefaultDashboardStatsExpiry),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ToggleUserStatus_WhenSuccessful_InvalidatesUserAndStatsCache()
        {
            // Arrange
            var token = GenerateJwt("admin@hi-trade.com");
            _mockBLL.Setup(b => b.ToggleUserStatus(5, "admin@hi-trade.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "User status updated" });

            var adminLogger = new Mock<ILogger<AdminService>>();
            var adminService = new AdminService(_mockBLL.Object, adminLogger.Object, _mockServiceProvider.Object, _mockCacheService.Object);

            // Act
            var result = await adminService.ToggleUserStatus(5, token, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            _mockCacheService.Verify(c => c.RemoveByPrefixAsync(CacheKeys.UserPrefix, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(CacheKeys.AdminDashboardStats, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
