using FluentAssertions;
using Hi_Trade.Models.Common;
using Hi_Trade.Services.Services;
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
    public sealed class RedisCacheServiceTests
    {
        [TestMethod]
        public async Task GetAsync_WhenRedisUnconfigured_ReturnsDefaultWithoutThrowing()
        {
            // Arrange
            var options = Options.Create(new RedisOptions { EndpointUrl = "" });
            var logger = new Mock<ILogger<RedisCacheService>>();
            using var service = new RedisCacheService(options, logger.Object);

            // Act
            var result = await service.GetAsync<string>("test-key", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task SetAsync_WhenRedisUnconfigured_DoesNotThrow()
        {
            // Arrange
            var options = Options.Create(new RedisOptions { EndpointUrl = "" });
            var logger = new Mock<ILogger<RedisCacheService>>();
            using var service = new RedisCacheService(options, logger.Object);

            // Act & Assert
            var act = async () => await service.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(5), CancellationToken.None);
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task RemoveAsync_WhenRedisUnconfigured_DoesNotThrow()
        {
            // Arrange
            var options = Options.Create(new RedisOptions { EndpointUrl = "" });
            var logger = new Mock<ILogger<RedisCacheService>>();
            using var service = new RedisCacheService(options, logger.Object);

            // Act & Assert
            var act = async () => await service.RemoveAsync("test-key", CancellationToken.None);
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_WhenRedisUnconfigured_ReturnsFalse()
        {
            // Arrange
            var options = Options.Create(new RedisOptions { EndpointUrl = "" });
            var logger = new Mock<ILogger<RedisCacheService>>();
            using var service = new RedisCacheService(options, logger.Object);

            // Act
            var exists = await service.ExistsAsync("test-key", CancellationToken.None);

            // Assert
            exists.Should().BeFalse();
        }

        [TestMethod]
        public async Task GetOrSetAsync_WhenRedisUnconfigured_ExecutesFactoryAndReturnsValue()
        {
            // Arrange
            var options = Options.Create(new RedisOptions { EndpointUrl = "" });
            var logger = new Mock<ILogger<RedisCacheService>>();
            using var service = new RedisCacheService(options, logger.Object);

            var factoryExecuted = false;

            // Act
            var result = await service.GetOrSetAsync("test-key", () =>
            {
                factoryExecuted = true;
                return Task.FromResult<string?>("fresh-data");
            });

            // Assert
            result.Should().Be("fresh-data");
            factoryExecuted.Should().BeTrue();
        }
    }
}
