using FluentAssertions;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class PortfolioServiceCreateTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL;
        private Mock<ILogger<PortfolioService>> _mockLogger;
        private Mock<IServiceProvider> _mockServiceProvider;
        private PortfolioService _portfolioService;

        [TestInitialize]
        public void Setup()
        {
            _mockHiTradeBLL = new Mock<IHiTradeBLL>();
            _mockLogger = new Mock<ILogger<PortfolioService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _portfolioService = new PortfolioService(
                _mockHiTradeBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object
            );
        }

        private string GenerateToken(string email)
        {
            return "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";
        }

        #region GetUserPortfolios Success Cases

        [TestMethod]
        public async Task GetUserPortfolios_WithValidToken_ReturnsSuccessWithPortfolios()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            var portfolios = new List<PortfolioDTO>
            {
                new PortfolioDTO
                {
                    Id = 1,
                    Name = "Main Portfolio",
                    CurrentValue = 5000.00m,
                    GainLoss = 500.00m,
                    Performance = 10.00m,
                    Positions = new List<PositionDTO>()
                },
                new PortfolioDTO
                {
                    Id = 2,
                    Name = "Aggressive Portfolio",
                    CurrentValue = 3000.00m,
                    GainLoss = -200.00m,
                    Performance = -6.25m,
                    Positions = new List<PositionDTO>()
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolios);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Should().HaveCount(2);
            result.Message.Should().BeNull();

            _mockHiTradeBLL.Verify(x => x.GetUserPortfolios(It.IsAny<string>(), ct), Times.Once);
        }

        [TestMethod]
        public async Task GetUserPortfolios_WithEmptyPortfolios_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PortfolioDTO>());

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetUserPortfolios_WithPositions_ReturnsPortfoliosWithPositions()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            var positions = new List<PositionDTO>
            {
                new PositionDTO
                {
                    Id = 1,
                    AveragePrice = 150.00m,
                    Quantity = 10,
                    Asset = new AssetDTO
                    {
                        Id = 1,
                        Ticker = "AAPL",
                        Name = "Apple Inc.",
                        CurrentPrice = 155.00m
                    }
                }
            };

            var portfolios = new List<PortfolioDTO>
            {
                new PortfolioDTO
                {
                    Id = 1,
                    Name = "Tech Portfolio",
                    CurrentValue = 1550.00m,
                    GainLoss = 50.00m,
                    Performance = 3.33m,
                    Positions = positions
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolios);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.Model[0].Positions.Should().HaveCount(1);
            result.Model[0].Positions[0].Asset.Ticker.Should().Be("AAPL");
        }

        #endregion

        #region GetUserPortfolios Failure Cases

        [TestMethod]
        public async Task GetUserPortfolios_WithException_ReturnsErrorResult()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Database error");

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

        #region BuyAsset Success Cases

        [TestMethod]
        public async Task BuyAsset_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new BuyAssetRequest
            {
                PortfolioId = 1,
                AssetId = 5,
                Quantity = 10,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Asset purchased successfully"
            };

            _mockHiTradeBLL.Setup(x => x.BuyAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.BuyAsset(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Success.Should().BeTrue();

            _mockHiTradeBLL.Verify(x => x.BuyAsset(request, ct), Times.Once);
        }

        [TestMethod]
        public async Task BuyAsset_WithInsufficientFunds_ReturnsFail()
        {
            // Arrange
            var request = new BuyAssetRequest
            {
                PortfolioId = 1,
                AssetId = 5,
                Quantity = 10000,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Not enough balance. Please add funds"
            };

            _mockHiTradeBLL.Setup(x => x.BuyAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.BuyAsset(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeFalse();
            result.Model.Message.Should().Contain("Not enough balance");
        }

        #endregion

        #region BuyAsset Failure Cases

        [TestMethod]
        public async Task BuyAsset_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new BuyAssetRequest
            {
                PortfolioId = 1,
                AssetId = 5,
                Quantity = 10,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.BuyAsset(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Portfolio not found"));

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.BuyAsset(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
        }

        #endregion

        #region SellAsset Success Cases

        [TestMethod]
        public async Task SellAsset_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new SellAssetRequest
            {
                PortfolioId = 1,
                PositionId = 5,
                Quantity = 5,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Asset sold successfully"
            };

            _mockHiTradeBLL.Setup(x => x.SellAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.SellAsset(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Success.Should().BeTrue();

            _mockHiTradeBLL.Verify(x => x.SellAsset(request, ct), Times.Once);
        }

        [TestMethod]
        public async Task SellAsset_WithInsufficientQuantity_ReturnsFail()
        {
            // Arrange
            var request = new SellAssetRequest
            {
                PortfolioId = 1,
                PositionId = 5,
                Quantity = 1000,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Not enough quantity to sell"
            };

            _mockHiTradeBLL.Setup(x => x.SellAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.SellAsset(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeFalse();
        }

        #endregion

        #region SellAsset Failure Cases

        [TestMethod]
        public async Task SellAsset_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new SellAssetRequest
            {
                PortfolioId = 1,
                PositionId = 5,
                Quantity = 5,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.SellAsset(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Position not found"));

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.SellAsset(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
        }

        #endregion

        #region Portfolio Performance Calculations

        [TestMethod]
        public async Task GetUserPortfolios_CalculatesPerformanceCorrectly()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            var portfolios = new List<PortfolioDTO>
            {
                new PortfolioDTO
                {
                    Id = 1,
                    Name = "Profitable Portfolio",
                    CurrentValue = 5500.00m,
                    GainLoss = 500.00m,
                    Performance = 10.00m  // (500 / 5000) * 100
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolios);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.Model[0].Performance.Should().Be(10.00m);
        }

        [TestMethod]
        public async Task GetUserPortfolios_HandleNegativePerformance()
        {
            // Arrange
            var token = GenerateToken("test@example.com");

            var portfolios = new List<PortfolioDTO>
            {
                new PortfolioDTO
                {
                    Id = 1,
                    Name = "Loss Portfolio",
                    CurrentValue = 4500.00m,
                    GainLoss = -500.00m,
                    Performance = -10.00m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetUserPortfolios(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolios);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetUserPortfolios(token, ct);

            // Assert
            result.Model[0].GainLoss.Should().Be(-500.00m);
            result.Model[0].Performance.Should().Be(-10.00m);
        }

        #endregion
    }
}
