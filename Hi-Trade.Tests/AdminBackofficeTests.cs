using FluentAssertions;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Controllers;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Responses;
using Hi_Trade.Services.Interfaces;
using Hi_Trade.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hi_Trade.Tests
{
    [TestClass]
    public class AdminBackofficeTests
    {
        private Mock<IHiTradeBLL> _mockBLL = null!;
        private Mock<ILogger<AdminService>> _mockLogger = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private AdminService _adminService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockBLL = new Mock<IHiTradeBLL>();
            _mockLogger = new Mock<ILogger<AdminService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _adminService = new AdminService(_mockBLL.Object, _mockLogger.Object, _mockServiceProvider.Object);
        }

        [TestMethod]
        public void AdminController_EnforcesAdminRole()
        {
            var controllerType = typeof(AdminController);
            var classAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            classAttr.Should().NotBeNull();
            classAttr!.Roles.Should().Be("Admin");

            var statsMethod = controllerType.GetMethod(nameof(AdminController.GetDashboardStats));
            var usersMethod = controllerType.GetMethod(nameof(AdminController.GetUsers));
            var toggleMethod = controllerType.GetMethod(nameof(AdminController.ToggleUserStatus));
            var transactionsMethod = controllerType.GetMethod(nameof(AdminController.GetTransactions));

            statsMethod.Should().NotBeNull();
            usersMethod.Should().NotBeNull();
            toggleMethod.Should().NotBeNull();
            transactionsMethod.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetDashboardStats_Success_ReturnsSuccessResult()
        {
            // Arrange
            var expectedStats = new AdminDashboardStatsDTO
            {
                TotalUsers = 15,
                TotalTraders = 14,
                TotalAdmins = 1,
                TotalPortfolios = 8,
                TotalAssets = 10,
                EnabledAssets = 8,
                DisabledAssets = 2,
                TotalTransactions = 25,
                TotalTradingVolume = 50000m,
                TotalUserBalances = 12000m,
                TotalDepositedAmount = 15000m
            };

            _mockBLL.Setup(b => b.GetAdminDashboardStats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _adminService.GetDashboardStats(CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().NotBeNull();
            result.Model!.TotalUsers.Should().Be(15);
            result.Model.TotalTraders.Should().Be(14);
            result.Model.TotalAdmins.Should().Be(1);
            result.Model.TotalTradingVolume.Should().Be(50000m);
        }

        [TestMethod]
        public async Task GetDashboardStats_WhenExceptionThrown_ReturnsErrorResult()
        {
            // Arrange
            _mockBLL.Setup(b => b.GetAdminDashboardStats(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _adminService.GetDashboardStats(CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Error);
            result.Model.Should().BeNull();
            result.Message.Should().Be("Database connection failed");
        }

        [TestMethod]
        public async Task GetUsers_Success_ReturnsUserList()
        {
            // Arrange
            var expectedUsers = new List<AdminUserDTO>
            {
                new AdminUserDTO { Id = 1, FullName = "Trader One", Email = "trader1@test.com", Role = "User", Enabled = true },
                new AdminUserDTO { Id = 2, FullName = "Admin User", Email = "admin@test.com", Role = "Admin", Enabled = true }
            };

            _mockBLL.Setup(b => b.GetAdminUsers("trader", null, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _adminService.GetUsers("trader", null, true, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task ToggleUserStatus_ExtractsAdminEmailAndCallsBLL()
        {
            // Arrange
            string token = CreateJwtToken("superadmin@test.com");
            _mockBLL.Setup(b => b.ToggleUserStatus(42, "superadmin@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveResponse { Success = true, Message = "User status updated" });

            // Act
            var result = await _adminService.ToggleUserStatus(42, token, CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model!.Success.Should().BeTrue();
            _mockBLL.Verify(b => b.ToggleUserStatus(42, "superadmin@test.com", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTransactions_Success_ReturnsTransactionsList()
        {
            // Arrange
            var expectedTransactions = new List<AdminTransactionDTO>
            {
                new AdminTransactionDTO
                {
                    Id = 1,
                    AssetTicker = "AAPL",
                    TransactionType = "Buy",
                    Quantity = 10,
                    PriceAtPurchase = 150m,
                    TotalAmount = 1500m
                }
            };

            _mockBLL.Setup(b => b.GetAdminTransactions("AAPL", "Buy", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTransactions);

            // Act
            var result = await _adminService.GetTransactions("AAPL", "Buy", CancellationToken.None);

            // Assert
            result.ResultType.Should().Be(ResultType.Success);
            result.Model.Should().HaveCount(1);
            result.Model![0].AssetTicker.Should().Be("AAPL");
            result.Model[0].TotalAmount.Should().Be(1500m);
        }

        private static string CreateJwtToken(string email)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("super_secret_unit_test_signing_key_123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, email),
                    new Claim("role", "Admin")
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}
