using BCrypt.Net;
using FluentValidation;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Common;
using Hi_Trade.DAL;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Hi_Trade.BLL.BLL
{
    public class HiTradeBLL(IHiTradeDAL hiTradeDAL, IConfiguration configuration) : IHiTradeBLL
    {
        public async Task<UserDTO> CreateUser(CreateUserRequest request, CancellationToken ct)
        {
            var hashedPassword = HashPassword(request.Password);
            var user = await hiTradeDAL.CreateUser(request.Email, hashedPassword, request.FullName, request.Address, ct);
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Balance = user.Balance,
                Role = user.Role
            };
        }
        public async Task SeedAdminAsync(AdminSeedOptions options, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
            {
                return;
            }
            if (await hiTradeDAL.AdminExistsAsync(ct))
            {
                return;
            }
            var hashedPassword = HashPassword(options.Password);
            await hiTradeDAL.CreateAdminUser(options.Email, hashedPassword, options.FullName, options.Address, ct);
        }
        public async Task<UserDTO?> LoginUser(LoginUserRequest request, CancellationToken ct)
        {
            var user = await hiTradeDAL.LoginUser(request.Email, ct);
            if (user == null)
            {
                return null;
            }
            else if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return null;
            }
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Balance = user.Balance,
                Role = user.Role
            };
        }
        public async Task<SaveResponse> CreateAsset(CreateAssetRequest request, CancellationToken ct)
        {
            var asset = await hiTradeDAL.CreateAsset(request.Ticker, request.Name, ct);
            return new SaveResponse
            {
                Success = asset != null,
                Message = asset != null ? "Asset created successfully" : "Failed to create asset"
            };
        }
        public async Task UpdateAssetPrices(CancellationToken ct)
        {
            var assets = await hiTradeDAL.GetAllAssets(ct);
            foreach (var asset in assets.Where(a => a.Enabled))
            {
                var newPrice = await GetNewPriceForAsset(asset.Ticker, ct);
                await hiTradeDAL.UpdateAssetPrice(asset.Id, newPrice, ct);
                Thread.Sleep(100);
            }
        }
        public async Task<decimal> GetNewPriceForAsset(string ticker, CancellationToken ct, int retries = 0)
        {
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", configuration["FinnhubToken"]);
            var response = await httpClient.GetAsync(Constants.FinnHubApi + ticker);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var priceData = System.Text.Json.JsonSerializer.Deserialize<FinnHubResponse>(content);
                if (priceData == null)
                {
                    if (retries < 5)
                    {
                        Thread.Sleep(1000 * (retries + 1));
                        return await GetNewPriceForAsset(ticker, ct, retries + 1);
                    }
                    throw new Exception("Failed to deserialize price data");
                }
                return priceData.c;
            }
            else
            {
                if (retries < 5)
                {
                    Thread.Sleep(1000 * (retries + 1));
                    return await GetNewPriceForAsset(ticker, ct, retries + 1);
                }
                throw new Exception("Failed to fetch price data");
            }
        }
        public async Task<List<PortfolioDTO>> GetUserPortfolios(string email, CancellationToken ct)
        {
            var portfolios = await hiTradeDAL.GetPortfolios(email, ct);
            List<PortfolioDTO> portfolioDTOs = new();
            foreach (Portfolio portfolio in portfolios)
            {
                PortfolioDTO portfolioDTO = new()
                {
                    Name = portfolio.Name,
                    Id = portfolio.Id,
                    CurrentValue = portfolio.Positions.Sum(p => p.Quantity * p.Asset.CurrentPrice)
                };
                portfolioDTO.GainLoss = portfolio.TotalRealizedGainLoss + portfolioDTO.CurrentValue - portfolio.Positions.Sum(p => p.Quantity * p.AveragePrice);
                portfolioDTO.Performance = portfolio.NetInvested != 0 ? (portfolioDTO.GainLoss / portfolio.NetInvested) * 100 : 0;
                portfolioDTO.Positions = portfolio.Positions.Select(p => new PositionDTO()
                {
                    AveragePrice = p.AveragePrice,
                    Quantity = p.Quantity,
                    Asset = MapAssetToDto(p.Asset),
                    Id = p.Id
                }).ToList();
                portfolioDTOs.Add(portfolioDTO);
            }
            return portfolioDTOs;
        }
        public async Task<SaveResponse> CreateUserPortfolio(CreatePortfolioRequest request, CancellationToken ct)
        {
            var result = new SaveResponse();
            (int success, result.Message) = await hiTradeDAL.CreateUserPortfolio(request.Name, request.Email!, ct);
            result.Success = success > 0;
            return result;
        }
        public async Task<SaveResponse> BuyAsset(BuyAssetRequest request, CancellationToken ct)
        {
            var result = new SaveResponse();
            (int success, result.Message) = await hiTradeDAL.BuyAsset(request.PortfolioId, request.AssetId, request.Quantity, request.Email!, ct);
            result.Success = success > 0;
            return result;
        }
        public async Task<SaveResponse> SellAsset(SellAssetRequest request, CancellationToken ct)
        {
            var result = new SaveResponse();
            (int success, result.Message) = await hiTradeDAL.SellAsset(request.PortfolioId, request.PositionId, request.Quantity, request.Email!, ct);
            result.Success = success > 0;
            return result;
        }
        public async Task<List<AssetDTO>> GetAllEnabledAssets(CancellationToken ct)
        {
            return (await hiTradeDAL.GetAllAssets(ct)).Where(a => a.Enabled).Select(MapAssetToDto).ToList();
        }
        public async Task<List<AssetDTO>> GetAllAssets(string? search, bool? enabled, CancellationToken ct)
        {
            return (await hiTradeDAL.GetAssetsFiltered(search, enabled, ct)).Select(MapAssetToDto).ToList();
        }
        public async Task<SaveResponse> ToggleAssetStatus(int assetId, CancellationToken ct)
        {
            var asset = await hiTradeDAL.ToggleAssetStatus(assetId, ct);
            return new SaveResponse
            {
                Success = true,
                Message = $"Asset {asset.Ticker} {(asset.Enabled ? "enabled" : "disabled")} successfully"
            };
        }
        private static AssetDTO MapAssetToDto(Asset asset)
        {
            return new AssetDTO()
            {
                Name = asset.Name,
                Ticker = asset.Ticker,
                CurrentPrice = asset.CurrentPrice,
                Id = asset.Id,
                Enabled = asset.Enabled
            };
        }
        public async Task<UserDTO> FetchUser(string email, CancellationToken ct)
        {
            User user = await hiTradeDAL.FetchUser(email, ct);
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Balance = user.Balance,
                Role = user.Role
            };
        }
        public async Task<SaveResponse> GetCheckoutLink(AddFundsRequest request, CancellationToken ct)
        {
            var client = new StripeClient(configuration["stripeRestrictedKey"]);
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount * 100,
                Currency = "eur",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            PaymentIntent intent = await client.V1.PaymentIntents.CreateAsync(options, cancellationToken: ct);
            SaveResponse response = new()
            {
                Success = true,
                Message = intent.ClientSecret
            };
            return response;
        }
        public async Task<SaveResponse> ConfirmPayment(ConfirmPaymentRequest request, CancellationToken ct)
        {
            SaveResponse result = new();
            var client = new StripeClient(configuration["stripeRestrictedKey"]);
            PaymentIntent intent = await client.V1.PaymentIntents.GetAsync(request.PaymentId, cancellationToken: ct);
            if(intent.Status == "succeeded")
            {
                (result.Success, result.Message) = await hiTradeDAL.AddFunds(request.PaymentId, request.Email, intent.Amount, ct);
            }
            else
            {
                result.Success = false;
                result.Message = "Payment not processed yet!";
            }
            return result;
        }
        public async Task<List<TransactionDTO>> GetPortfolioTransactions(GetPortfolioTransactionsRequest request, CancellationToken ct)
        {
            var transactions = await hiTradeDAL.GetPortfolioTransactions(request.PortfolioId, request.Email, ct);
            return transactions.Select(t => new TransactionDTO
            {
                Id = t.Id,
                AssetId = t.Asset.Id,
                AssetTicker = t.Asset.Ticker,
                AssetName = t.Asset.Name,
                Quantity = t.Quantity,
                PriceAtTransaction = t.PriceAtPurchase,
                TransactionType = t.Type.ToString(),
                CreatedAt = t.CreatedAt,
                TotalValue = t.Quantity * t.PriceAtPurchase
            }).OrderByDescending(t => t.CreatedAt).ToList();
        }
        public async Task<UserDTO> EditProfile(EditProfileRequest request, CancellationToken ct)
        {
            string profilePicPath = request.ProfilePictureUrl;
            if (!string.IsNullOrEmpty(request.ProfilePictureUrl))
            {
                if (request.ProfilePictureUrl.Contains(','))
                {
                    request.ProfilePictureUrl = request.ProfilePictureUrl.Split(',')[1];
                }
                string extension = Constants.GetFileExtension(request.ProfilePictureUrl);
                if (extension != "jpg" && extension != "png")
                {
                    throw new ValidationException("File is not an image");
                }
                byte[] imageBytes = Convert.FromBase64String(request.ProfilePictureUrl);
                profilePicPath = Constants.StaticFilesPath + Constants.ProfilePicsPath + Guid.NewGuid() + "." + extension;
                string? directoryPath = Path.GetDirectoryName(profilePicPath);   

                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await System.IO.File.WriteAllBytesAsync(profilePicPath, imageBytes, ct);
            }
            User user = await hiTradeDAL.EditProfile(request.OldEmail, request.Email, request.FullName, request.Address, profilePicPath, ct);
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Balance = user.Balance,
                Role = user.Role
            };
        }
        public async Task<AdminDashboardStatsDTO> GetAdminDashboardStats(CancellationToken ct)
        {
            var metrics = await hiTradeDAL.GetPlatformMetrics(ct);
            var recentTransactionsEntities = await hiTradeDAL.GetRecentTransactions(10, ct);
            var recentUsersEntities = await hiTradeDAL.GetRecentUsers(10, ct);

            var recentTransactions = recentTransactionsEntities.Select(t => new AdminTransactionDTO
            {
                Id = t.Id,
                TransactionDate = t.CreatedAt,
                UserFullName = t.Portfolio?.User?.FullName ?? string.Empty,
                UserEmail = t.Portfolio?.User?.Email ?? string.Empty,
                PortfolioName = t.Portfolio?.Name ?? string.Empty,
                AssetTicker = t.Asset?.Ticker ?? string.Empty,
                AssetName = t.Asset?.Name ?? string.Empty,
                TransactionType = t.Type.ToString(),
                Quantity = t.Quantity,
                PriceAtPurchase = t.PriceAtPurchase,
                TotalAmount = t.Quantity * t.PriceAtPurchase
            }).ToList();

            var recentUsers = recentUsersEntities.Select(u => new AdminUserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Address = u.Address,
                Balance = u.Balance,
                Role = u.Role.ToString(),
                PortfoliosCount = u.Portfolios.Count,
                CreatedAt = u.CreatedAt,
                Enabled = u.Enabled
            }).ToList();

            return new AdminDashboardStatsDTO
            {
                TotalUsers = metrics.TotalUsers,
                TotalTraders = metrics.TotalTraders,
                TotalAdmins = metrics.TotalAdmins,
                TotalPortfolios = metrics.TotalPortfolios,
                TotalAssets = metrics.TotalAssets,
                EnabledAssets = metrics.EnabledAssets,
                DisabledAssets = metrics.DisabledAssets,
                TotalTransactions = metrics.TotalTransactions,
                BuyTransactionsCount = metrics.BuyTransactionsCount,
                SellTransactionsCount = metrics.SellTransactionsCount,
                TotalTradingVolume = metrics.TotalTradingVolume,
                TotalUserBalances = metrics.TotalUserBalances,
                TotalDepositedAmount = metrics.TotalDepositedAmount,
                RecentTransactions = recentTransactions,
                RecentUsers = recentUsers
            };
        }
        public async Task<List<AdminUserDTO>> GetAdminUsers(string? search, string? role, bool? enabled, CancellationToken ct)
        {
            Roles? roleFilter = null;
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<Roles>(role, true, out var parsedRole))
            {
                roleFilter = parsedRole;
            }

            var users = await hiTradeDAL.GetUsersFiltered(search, roleFilter, enabled, ct);
            return users.Select(u => new AdminUserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Address = u.Address,
                Balance = u.Balance,
                Role = u.Role.ToString(),
                PortfoliosCount = u.Portfolios.Count,
                CreatedAt = u.CreatedAt,
                Enabled = u.Enabled
            }).ToList();
        }
        public async Task<SaveResponse> ToggleUserStatus(int userId, string adminEmail, CancellationToken ct)
        {
            var user = await hiTradeDAL.ToggleUserStatus(userId, adminEmail, ct);
            return new SaveResponse
            {
                Success = true,
                Message = $"User {user.Email} is now {(user.Enabled ? "enabled" : "disabled")}."
            };
        }
        public async Task<List<AdminTransactionDTO>> GetAdminTransactions(string? search, string? type, CancellationToken ct)
        {
            TransactionType? typeFilter = null;
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
            {
                typeFilter = parsedType;
            }

            var transactions = await hiTradeDAL.GetAllTransactions(search, typeFilter, ct);
            return transactions.Select(t => new AdminTransactionDTO
            {
                Id = t.Id,
                TransactionDate = t.CreatedAt,
                UserFullName = t.Portfolio?.User?.FullName ?? string.Empty,
                UserEmail = t.Portfolio?.User?.Email ?? string.Empty,
                PortfolioName = t.Portfolio?.Name ?? string.Empty,
                AssetTicker = t.Asset?.Ticker ?? string.Empty,
                AssetName = t.Asset?.Name ?? string.Empty,
                TransactionType = t.Type.ToString(),
                Quantity = t.Quantity,
                PriceAtPurchase = t.PriceAtPurchase,
                TotalAmount = t.Quantity * t.PriceAtPurchase
            }).ToList();
        }
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
