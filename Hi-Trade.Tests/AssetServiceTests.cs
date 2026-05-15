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
    public sealed class AssetServiceTests
    {
        private Mock<IHiTradeBLL> _mockHiTradeBLL;
        private Mock<ILogger<AssetService>> _mockLogger;
        private Mock<IServiceProvider> _mockServiceProvider;
        private AssetService _assetService;

        [TestInitialize]
        public void Setup()
        {
            _mockHiTradeBLL = new Mock<IHiTradeBLL>();
            _mockLogger = new Mock<ILogger<AssetService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _assetService = new AssetService(
                _mockHiTradeBLL.Object,
                _mockLogger.Object,
                _mockServiceProvider.Object
            );
        }

        #region GetAllEnabledAssets Success Cases

        [TestMethod]
        public async Task GetAllEnabledAssets_WithMultipleAssets_ReturnsSuccessWithAssets()
        {
            // Arrange
            var assets = new List<AssetDTO>
            {
                new AssetDTO
                {
                    Id = 1,
                    Ticker = "AAPL",
                    Name = "Apple Inc.",
                    CurrentPrice = 150.50m
                },
                new AssetDTO
                {
                    Id = 2,
                    Ticker = "MSFT",
                    Name = "Microsoft",
                    CurrentPrice = 305.00m
                },
                new AssetDTO
                {
                    Id = 3,
                    Ticker = "GOOGL",
                    Name = "Alphabet Inc.",
                    CurrentPrice = 140.00m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(assets);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Should().HaveCount(3);
            result.Message.Should().BeNull();

            _mockHiTradeBLL.Verify(x => x.GetAllEnabledAssets(ct), Times.Once);
        }

        [TestMethod]
        public async Task GetAllEnabledAssets_WithEmptyAssetList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetDTO>());

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllEnabledAssets_WithHighPricedAssets_ReturnsSuccessPreservingPrices()
        {
            // Arrange
            var assets = new List<AssetDTO>
            {
                new AssetDTO
                {
                    Id = 1,
                    Ticker = "BRK.A",
                    Name = "Berkshire Hathaway",
                    CurrentPrice = 550000.00m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(assets);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            result.Model[0].CurrentPrice.Should().Be(550000.00m);
        }

        #endregion

        #region GetAllEnabledAssets Failure Cases

        [TestMethod]
        public async Task GetAllEnabledAssets_WithException_ReturnsErrorResult()
        {
            // Arrange
            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

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
        public async Task GetAllEnabledAssets_WithOperationCanceledException_ReturnsErrorResult()
        {
            // Arrange
            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _assetService.GetAllEnabledAssets(cts.Token);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
        }

        #endregion

        #region CreateAsset Success Cases

        [TestMethod]
        public async Task CreateAsset_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                Ticker = "TSLA",
                Name = "Tesla Inc."
            };

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Asset created successfully"
            };

            _mockHiTradeBLL.Setup(x => x.CreateAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.CreateAsset(request, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Success.Should().BeTrue();

            _mockHiTradeBLL.Verify(x => x.CreateAsset(request, ct), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsset_WithValidTicker_CreatesAssetSuccessfully()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                Ticker = "NIO",
                Name = "NIO Inc."
            };

            var saveResponse = new SaveResponse
            {
                Success = true,
                Message = "Asset created successfully"
            };

            _mockHiTradeBLL.Setup(x => x.CreateAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.CreateAsset(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeTrue();
        }

        #endregion

        #region CreateAsset Failure Cases

        [TestMethod]
        public async Task CreateAsset_WithDuplicateTicker_ReturnsFail()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                Ticker = "AAPL",
                Name = "Apple Inc."
            };

            var saveResponse = new SaveResponse
            {
                Success = false,
                Message = "Asset with the same ticker already exists"
            };

            _mockHiTradeBLL.Setup(x => x.CreateAsset(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.CreateAsset(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Success.Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateAsset_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new CreateAssetRequest
            {
                Ticker = "TEST",
                Name = "Test Asset"
            };

            _mockHiTradeBLL.Setup(x => x.CreateAsset(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.CreateAsset(request, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Database error");
        }

        #endregion

        #region Asset Data Integrity

        [TestMethod]
        public async Task GetAllEnabledAssets_PreservesAllAssetData()
        {
            // Arrange
            var assets = new List<AssetDTO>
            {
                new AssetDTO
                {
                    Id = 42,
                    Ticker = "TEST",
                    Name = "Test Company",
                    CurrentPrice = 123.45m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(assets);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            var asset = result.Model[0];
            asset.Id.Should().Be(42);
            asset.Ticker.Should().Be("TEST");
            asset.Name.Should().Be("Test Company");
            asset.CurrentPrice.Should().Be(123.45m);
        }

        [TestMethod]
        public async Task GetAllEnabledAssets_PreservesPricePrecision()
        {
            // Arrange
            var assets = new List<AssetDTO>
            {
                new AssetDTO
                {
                    Id = 1,
                    Ticker = "AAPL",
                    Name = "Apple Inc.",
                    CurrentPrice = 150.123456m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(assets);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            result.Model[0].CurrentPrice.Should().Be(150.123456m);
        }

        #endregion

        #region Large Data Set Tests

        [TestMethod]
        public async Task GetAllEnabledAssets_WithLargeAssetList_ReturnsAllAssets()
        {
            // Arrange
            var assets = Enumerable.Range(1, 1000)
                .Select(i => new AssetDTO
                {
                    Id = i,
                    Ticker = $"TICK{i}",
                    Name = $"Company {i}",
                    CurrentPrice = 100.00m + i
                }).ToList();

            _mockHiTradeBLL.Setup(x => x.GetAllEnabledAssets(It.IsAny<CancellationToken>()))
                .ReturnsAsync(assets);

            var ct = CancellationToken.None;

            // Act
            var result = await _assetService.GetAllEnabledAssets(ct);

            // Assert
            result.Model.Should().HaveCount(1000);
            result.ResultType.Should().Be(ResultType.Success);
        }

        #endregion
    }
}
