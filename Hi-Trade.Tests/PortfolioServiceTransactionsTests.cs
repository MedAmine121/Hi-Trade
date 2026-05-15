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
    public sealed class PortfolioServiceTransactionsTests
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
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIudGVzdEBlbWFpbC5jb20ifQ.test";
            return token;
        }

        #region GetPortfolioTransactions Success Cases

        [TestMethod]
        public async Task GetPortfolioTransactions_WithValidRequest_ReturnsSuccessWithTransactions()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var transactions = new List<TransactionDTO>
            {
                new TransactionDTO
                {
                    Id = 1,
                    AssetId = 5,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 10,
                    PriceAtTransaction = 150.50m,
                    TransactionType = "Buy",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    TotalValue = 1505.00m
                },
                new TransactionDTO
                {
                    Id = 2,
                    AssetId = 5,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 5,
                    PriceAtTransaction = 155.00m,
                    TransactionType = "Sell",
                    CreatedAt = DateTime.UtcNow,
                    TotalValue = 775.00m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Should().HaveCount(2);
            result.Message.Should().BeNull();

            _mockHiTradeBLL.Verify(x => x.GetPortfolioTransactions(request, ct), Times.Once);
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_WithEmptyTransactionList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");
            var transactions = new List<TransactionDTO>();

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_WithMultipleAssets_ReturnsAllTransactions()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 2,
                Email = "user@example.com"
            };

            var token = GenerateToken("user@example.com");

            var transactions = new List<TransactionDTO>
            {
                new TransactionDTO
                {
                    Id = 1,
                    AssetId = 1,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 10,
                    PriceAtTransaction = 150.00m,
                    TransactionType = "Buy",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    TotalValue = 1500.00m
                },
                new TransactionDTO
                {
                    Id = 2,
                    AssetId = 2,
                    AssetTicker = "MSFT",
                    AssetName = "Microsoft",
                    Quantity = 20,
                    PriceAtTransaction = 300.00m,
                    TransactionType = "Buy",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    TotalValue = 6000.00m
                },
                new TransactionDTO
                {
                    Id = 3,
                    AssetId = 1,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 5,
                    PriceAtTransaction = 155.00m,
                    TransactionType = "Sell",
                    CreatedAt = DateTime.UtcNow,
                    TotalValue = 775.00m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().HaveCount(3);
            result.Model.Should().Contain(t => t.AssetTicker == "AAPL");
            result.Model.Should().Contain(t => t.AssetTicker == "MSFT");
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_VerifiesCorrectPortfolioIdIsPassed()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 42,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(It.IsAny<GetPortfolioTransactionsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionDTO>());

            var ct = CancellationToken.None;

            // Act
            await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            _mockHiTradeBLL.Verify(
                x => x.GetPortfolioTransactions(It.Is<GetPortfolioTransactionsRequest>(r => r.PortfolioId == 42), ct),
                Times.Once);
        }

        #endregion

        #region GetPortfolioTransactions Failure Cases

        [TestMethod]
        public async Task GetPortfolioTransactions_WithException_ReturnsErrorResult()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Should().NotBeNull();
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

        [TestMethod]
        public async Task GetPortfolioTransactions_WithPortfolioNotFound_ReturnsErrorResult()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 999,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Portfolio not found"));

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
        }

        #endregion

        #region GetPortfolioTransactions Edge Cases

        [TestMethod]
        public async Task GetPortfolioTransactions_WithLargeTransactionList_ReturnsAllTransactions()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var transactions = Enumerable.Range(1, 100)
                .Select(i => new TransactionDTO
                {
                    Id = i,
                    AssetId = 1,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 1,
                    PriceAtTransaction = 150.00m,
                    TransactionType = i % 2 == 0 ? "Buy" : "Sell",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    TotalValue = 150.00m
                }).ToList();

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Should().NotBeNull();
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().HaveCount(100);
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_WithOperationCanceledException_ReturnsErrorResult()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, cts.Token);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_VerifiesLoggingOnSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionDTO>());

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
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

        #region Transaction Data Integrity

        [TestMethod]
        public async Task GetPortfolioTransactions_PreservesAllTransactionData()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var createdAt = DateTime.UtcNow;
            var transactions = new List<TransactionDTO>
            {
                new TransactionDTO
                {
                    Id = 123,
                    AssetId = 456,
                    AssetTicker = "AAPL",
                    AssetName = "Apple Inc.",
                    Quantity = 25.5m,
                    PriceAtTransaction = 150.75m,
                    TransactionType = "Buy",
                    CreatedAt = createdAt,
                    TotalValue = 3831.375m
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Model.Should().HaveCount(1);
            var transaction = result.Model[0];
            transaction.Id.Should().Be(123);
            transaction.AssetId.Should().Be(456);
            transaction.AssetTicker.Should().Be("AAPL");
            transaction.AssetName.Should().Be("Apple Inc.");
            transaction.Quantity.Should().Be(25.5m);
            transaction.PriceAtTransaction.Should().Be(150.75m);
            transaction.TransactionType.Should().Be("Buy");
            transaction.CreatedAt.Should().Be(createdAt);
            transaction.TotalValue.Should().Be(3831.375m);
        }

        [TestMethod]
        public async Task GetPortfolioTransactions_MaintainsDifferentTransactionTypes()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            var token = GenerateToken("test@example.com");

            var transactions = new List<TransactionDTO>
            {
                new TransactionDTO
                {
                    Id = 1,
                    AssetId = 1,
                    AssetTicker = "AAPL",
                    AssetName = "Apple",
                    Quantity = 10,
                    PriceAtTransaction = 150,
                    TransactionType = "Buy",
                    CreatedAt = DateTime.UtcNow,
                    TotalValue = 1500
                },
                new TransactionDTO
                {
                    Id = 2,
                    AssetId = 1,
                    AssetTicker = "AAPL",
                    AssetName = "Apple",
                    Quantity = 5,
                    PriceAtTransaction = 155,
                    TransactionType = "Sell",
                    CreatedAt = DateTime.UtcNow,
                    TotalValue = 775
                }
            };

            _mockHiTradeBLL.Setup(x => x.GetPortfolioTransactions(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var ct = CancellationToken.None;

            // Act
            var result = await _portfolioService.GetPortfolioTransactions(request, token, ct);

            // Assert
            result.Model.Should().Contain(t => t.TransactionType == "Buy");
            result.Model.Should().Contain(t => t.TransactionType == "Sell");
        }

        #endregion
    }
}
