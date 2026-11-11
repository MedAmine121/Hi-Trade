using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Hi_Trade.BLL.BLL;
using Hi_Trade.Controllers;
using Hi_Trade.DAL;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Hi_Trade.Tests
{
    [TestClass]
    public class AdminRoleTests
    {
        private Mock<IHiTradeDAL> _mockDAL = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private HiTradeBLL _bll = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockDAL = new Mock<IHiTradeDAL>();
            _mockConfiguration = new Mock<IConfiguration>();
            _bll = new HiTradeBLL(_mockDAL.Object, _mockConfiguration.Object);
        }

        [TestMethod]
        public async Task SeedAdminAsync_WhenNoAdminExists_CreatesAdminWithHashedPassword()
        {
            // Arrange
            _mockDAL.Setup(x => x.AdminExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            string? capturedHash = null;
            _mockDAL.Setup(x => x.CreateAdminUser(
                    "admin@hi-trade.com",
                    It.IsAny<string>(),
                    "System Admin",
                    "Hi-Trade HQ",
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, string, string, CancellationToken>((_, hash, _, _, _) => capturedHash = hash)
                .ReturnsAsync(new User { Id = 1, Email = "admin@hi-trade.com", Role = Roles.Admin });

            var options = new AdminSeedOptions
            {
                Email = "admin@hi-trade.com",
                Password = "Admin@123456!",
                FullName = "System Admin",
                Address = "Hi-Trade HQ"
            };

            // Act
            await _bll.SeedAdminAsync(options, CancellationToken.None);

            // Assert
            _mockDAL.Verify(x => x.CreateAdminUser(
                "admin@hi-trade.com",
                It.IsAny<string>(),
                "System Admin",
                "Hi-Trade HQ",
                It.IsAny<CancellationToken>()), Times.Once);
            capturedHash.Should().NotBeNullOrWhiteSpace();
            BCrypt.Net.BCrypt.Verify("Admin@123456!", capturedHash!).Should().BeTrue();
        }

        [TestMethod]
        public async Task SeedAdminAsync_WhenAdminAlreadyExists_SkipsCreation()
        {
            // Arrange
            _mockDAL.Setup(x => x.AdminExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var options = new AdminSeedOptions { Email = "admin@hi-trade.com", Password = "Admin@123456!" };

            // Act
            await _bll.SeedAdminAsync(options, CancellationToken.None);

            // Assert
            _mockDAL.Verify(x => x.CreateAdminUser(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task SeedAdminAsync_WhenPasswordBlank_SkipsDatabaseChecks()
        {
            // Arrange
            var options = new AdminSeedOptions { Email = "admin@hi-trade.com", Password = "" };

            // Act
            await _bll.SeedAdminAsync(options, CancellationToken.None);

            // Assert
            _mockDAL.Verify(x => x.AdminExistsAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockDAL.Verify(x => x.CreateAdminUser(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetAllAssets_MapsEnabledPropertyAndPassesFilters()
        {
            // Arrange
            _mockDAL.Setup(x => x.GetAssetsFiltered("NV", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Asset>
                {
                    new Asset { Id = 10, Ticker = "NVDA", Name = "NVIDIA", CurrentPrice = 120.5m, Enabled = false }
                });

            // Act
            var result = await _bll.GetAllAssets("NV", false, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Ticker.Should().Be("NVDA");
            result[0].Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task ToggleAssetStatus_ReturnsDescriptiveSaveResponse()
        {
            // Arrange
            _mockDAL.Setup(x => x.ToggleAssetStatus(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Asset { Id = 5, Ticker = "MSFT", Enabled = false });

            // Act
            var response = await _bll.ToggleAssetStatus(5, CancellationToken.None);

            // Assert
            response.Success.Should().BeTrue();
            response.Message.Should().Be("Asset MSFT disabled successfully");
        }

        [TestMethod]
        public async Task TokenBLL_GenerateJwtToken_EmitsRoleClaimCompatibleWithJwtBearer()
        {
            // Arrange
            const string secret = "super_secret_unit_test_signing_key_123456";
            var jwtOptions = Options.Create(new JWTOptions
            {
                Secret = secret,
                Issuer = "HiTrade",
                Audience = "HiTradeClient",
                ExpirationMinutes = 60
            });
            var tokenBll = new TokenBLL(jwtOptions);
            var readHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var bearerHandler = new JsonWebTokenHandler { MapInboundClaims = false };
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "HiTrade",
                ValidAudience = "HiTradeClient",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.Sub
            };

            // Act
            string adminToken = tokenBll.GenerateJwtToken(new UserDTO { Email = "admin@hi-trade.com", Role = Roles.Admin }).Token;
            string userToken = tokenBll.GenerateJwtToken(new UserDTO { Email = "trader@hi-trade.com", Role = Roles.User }).Token;

            var parsedAdmin = readHandler.ReadJwtToken(adminToken);
            var parsedUser = readHandler.ReadJwtToken(userToken);
            var adminValidation = await bearerHandler.ValidateTokenAsync(adminToken, validationParams);
            var userValidation = await bearerHandler.ValidateTokenAsync(userToken, validationParams);

            // Assert wire claims
            parsedAdmin.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
            parsedAdmin.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "admin@hi-trade.com");
            parsedUser.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");

            // Assert ASP.NET Core JwtBearer role validation
            adminValidation.IsValid.Should().BeTrue();
            var adminPrincipal = new ClaimsPrincipal(adminValidation.ClaimsIdentity);
            adminPrincipal.IsInRole("Admin").Should().BeTrue();
            adminPrincipal.IsInRole("User").Should().BeFalse();
            adminPrincipal.Identity?.Name.Should().Be("admin@hi-trade.com");

            userValidation.IsValid.Should().BeTrue();
            var userPrincipal = new ClaimsPrincipal(userValidation.ClaimsIdentity);
            userPrincipal.IsInRole("User").Should().BeTrue();
            userPrincipal.IsInRole("Admin").Should().BeFalse();
        }

        [TestMethod]
        public void AssetController_EnforcesAdminOnlyOnManagementActions()
        {
            var type = typeof(AssetController);

            var createAttr = type.GetMethod(nameof(AssetController.CreateAsset))!.GetCustomAttribute<AuthorizeAttribute>();
            var getAllAttr = type.GetMethod(nameof(AssetController.GetAllAssets))!.GetCustomAttribute<AuthorizeAttribute>();
            var toggleAttr = type.GetMethod(nameof(AssetController.ToggleAssetStatus))!.GetCustomAttribute<AuthorizeAttribute>();
            var getEnabledAttr = type.GetMethod(nameof(AssetController.GetEnabledAssets))!.GetCustomAttribute<AuthorizeAttribute>();

            createAttr.Should().NotBeNull();
            createAttr!.Roles.Should().Be("Admin");

            getAllAttr.Should().NotBeNull();
            getAllAttr!.Roles.Should().Be("Admin");

            toggleAttr.Should().NotBeNull();
            toggleAttr!.Roles.Should().Be("Admin");

            getEnabledAttr.Should().NotBeNull();
            getEnabledAttr!.Roles.Should().BeNull();
        }

        [TestMethod]
        public void PortfolioAndUserControllers_EnforceExpectedRoleAuthorization()
        {
            var portfolioClassAttr = typeof(PortfolioController).GetCustomAttribute<AuthorizeAttribute>();
            portfolioClassAttr.Should().NotBeNull();
            portfolioClassAttr!.Roles.Should().Be("User");

            var userType = typeof(UserController);
            userType.GetMethod(nameof(UserController.GetCheckoutLink))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("User");
            userType.GetMethod(nameof(UserController.ConfirmPayment))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("User");

            userType.GetMethod(nameof(UserController.FetchUser))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().BeNull();
            userType.GetMethod(nameof(UserController.LogoutUser))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().BeNull();
            userType.GetMethod(nameof(UserController.EditProfile))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().BeNull();

            userType.GetMethod(nameof(UserController.CreateUser))!.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
            userType.GetMethod(nameof(UserController.LoginUser))!.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
        }
    }
}
