using Hi_Trade.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL
{
    public class HiTradeDAL(HiTradeContext context) : IHiTradeDAL
    {
        public async Task<User> CreateUser(string email, string password, string fullName, string address, CancellationToken ct)
        {
            return await CreateUserWithRole(email, password, fullName, address, Roles.User, ct);
        }
        public async Task<bool> AdminExistsAsync(CancellationToken ct)
        {
            return await context.Users.AnyAsync(u => u.Role == Roles.Admin, ct);
        }
        public async Task<User> CreateAdminUser(string email, string password, string fullName, string address, CancellationToken ct)
        {
            return await CreateUserWithRole(email, password, fullName, address, Roles.Admin, ct);
        }
        private async Task<User> CreateUserWithRole(string email, string password, string fullName, string address, Roles role, CancellationToken ct)
        {
            User user = new()
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Address = address,
                Role = role
            };
            if (await context.Users.AnyAsync(u => u.Email == email, ct))
            {
                throw new Exception("User with the same email already exists.");
            }
            context.Users.Add(user);
            int result = await context.SaveChangesAsync(ct);
            if (result > 0)
            {
                return user;
            }
            else
            {
                throw new Exception("Failed to create user.");
            }
        }
        public async Task<User?> LoginUser(string email, CancellationToken ct)
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            return user;
        }
        public async Task<Asset> CreateAsset(string ticker, string name, CancellationToken ct)
        {
            Asset asset = new()
            {
                Ticker = ticker,
                Name = name
            };
            if (await context.Assets.AnyAsync(a => a.Ticker == ticker, ct))
            {
                throw new Exception("Asset with the same ticker already exists.");
            }
            context.Assets.Add(asset);
            int result = await context.SaveChangesAsync(ct);
            if (result > 0)
            {
                return asset;
            }
            else
            {
                throw new Exception("Failed to create asset.");
            }
        }
        public async Task<List<Asset>> GetAllAssets(CancellationToken ct)
        {
            return await context.Assets.ToListAsync(ct);
        }
        public async Task<List<Asset>> GetAssetsFiltered(string? search, bool? enabled, CancellationToken ct)
        {
            var query = context.Assets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(a => a.Ticker.ToLower().Contains(term) || a.Name.ToLower().Contains(term));
            }

            if (enabled.HasValue)
            {
                query = query.Where(a => a.Enabled == enabled.Value);
            }

            return await query.OrderBy(a => a.Ticker).ToListAsync(ct);
        }
        public async Task<Asset> ToggleAssetStatus(int assetId, CancellationToken ct)
        {
            var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct)
                ?? throw new Exception("Asset not found.");
            asset.Enabled = !asset.Enabled;
            context.Assets.Update(asset);
            int result = await context.SaveChangesAsync(ct);
            if (result <= 0)
            {
                throw new Exception("Failed to toggle asset status.");
            }
            return asset;
        }
        public async Task UpdateAssetPrice(int assetId, decimal newPrice, CancellationToken ct)
        {
            var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct);
            if (asset == null)
            {
                throw new Exception("Asset not found.");
            }
            asset.CurrentPrice = newPrice;
            context.Assets.Update(asset);
            int result = await context.SaveChangesAsync(ct);
            if (result <= 0)
            {
                throw new Exception("Failed to update asset price.");
            }
        }
        public async Task<List<Portfolio>> GetPortfolios(string email, CancellationToken ct)
        {
            return await context.Portfolios.Where(p => p.User.Email == email).Include(p => p.Positions).Include(p => p.Transactions).ThenInclude(t => t.Asset).ToListAsync(ct);
        }
        public async Task<(int,string)> CreateUserPortfolio(string name, string email, CancellationToken ct)
        {
            var user = await context.Users.Include(u => u.Portfolios).FirstOrDefaultAsync(u => u.Email.Equals(email), ct);
            if(user == null)
            {
                throw new Exception("User not found");
            }
            var portfolioExists = user.Portfolios.Any(p => p.Name == name);
            if(portfolioExists)
            {
                return (0, "Portfolio already exists. Please choose a different name");
            }
            var portfolio = new Portfolio()
            {
                Name = name,
                User = user
            };
            context.Portfolios.Add(portfolio);
            int result = await context.SaveChangesAsync(ct);
            if(result <= 0)
            {
                throw new Exception("Failed to create portfolio");
            }
            return (result, "");
        }
        public async Task<(int, string)> BuyAsset(int portfolioId, int assetId, decimal quantity, string email, CancellationToken ct)
        {
            var user = await context.Users.Include(u => u.Portfolios).ThenInclude(p => p.Positions).ThenInclude(p => p.Asset).FirstOrDefaultAsync(u => u.Email == email, ct);
            if(user == null)
            {
                throw new Exception("User not found");
            }
            var portfolio = user.Portfolios.FirstOrDefault(p => p.Id == portfolioId);
            if(portfolio == null)
            {
                throw new Exception("Portfolio not found");
            }
            var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId, ct);
            if(asset == null)
            {
                throw new Exception("Asset not found");
            }
            if(!asset.Enabled)
            {
                return (0, "Asset is currently disabled");
            }
            if(user.Balance < asset.CurrentPrice * quantity)
            {
                return (0, "Not enough balance. Please add funds");
            }
            Position? position = portfolio.Positions.FirstOrDefault(p => p.Asset.Id == assetId);
            if(position != null)
            {
                position.AveragePrice = ((position.AveragePrice * position.Quantity) + (asset.CurrentPrice * quantity)) / (position.Quantity + quantity);
                position.Quantity += quantity;
            }
            else
            {
                position = new() { 
                    Asset = asset,
                    Quantity = quantity,
                    AveragePrice = asset.CurrentPrice,
                    Portfolio = portfolio
                };
                portfolio.Positions.Add(position);
            }
            user.Balance -= asset.CurrentPrice * quantity;
            Transaction transaction = new()
            {
                Type = TransactionType.Buy,
                Asset = asset,
                Portfolio = portfolio,
                Quantity = quantity,
                PriceAtPurchase = asset.CurrentPrice
            };
            portfolio.Transactions.Add(transaction);
            portfolio.NetInvested += asset.CurrentPrice * quantity;
            int result = await context.SaveChangesAsync(ct);
            return (result, "");
        }
        public async Task<(int, string)> SellAsset(int portfolioId, int positionId, decimal quantity, string email, CancellationToken ct)
        {
            var user = await context.Users.Include(u => u.Portfolios).ThenInclude(p => p.Positions).ThenInclude(p => p.Asset).FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var portfolio = user.Portfolios.FirstOrDefault(p => p.Id == portfolioId);
            if (portfolio == null)
            {
                throw new Exception("Portfolio not found");
            }
            var position = portfolio.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
            {
                throw new Exception("Position not found");
            }
            if (position.Quantity < quantity)
            {
                return (0, "Not enough quantity to sell");
            }
            else if(position.Quantity == quantity)
            {
                portfolio.Positions.Remove(position);
            }
            else
            {
                position.Quantity -= quantity;
            }
            Asset? asset = position.Asset;
            portfolio.TotalRealizedGainLoss += (asset.CurrentPrice - position.AveragePrice) * quantity;
            user.Balance += asset.CurrentPrice * quantity;
            Transaction transaction = new()
            {
                Type = TransactionType.Sell,
                Asset = asset,
                Portfolio = portfolio,
                Quantity = quantity,
                PriceAtPurchase = asset.CurrentPrice
            };
            portfolio.Transactions.Add(transaction);
            portfolio.NetInvested -= (position.AveragePrice * quantity);
            int result = await context.SaveChangesAsync(ct);
            return (result, "");
        }
        public async Task<User> FetchUser(string email, CancellationToken ct)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if(user == null)
            {
                throw new Exception("User does not exist");
            }
            return user;
        }
        public async Task<(bool, string)> AddFunds(string paymentId, string email, long amount, CancellationToken ct)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct) ?? throw new Exception("User does not exist");
            var payment = await context.PaymentTraces.FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
            if(payment != null && payment.Status == "succeeded")
            {
                throw new Exception("Payment already processed!");
            }
            else if(payment != null)
            {
                payment.Status = "succeeded";
            }
            else
            {
                payment = new()
                {
                    Status = "succeeded",
                    PaymentId = paymentId,
                    User = user,
                    Amount = amount
                };
                user.payments.Add(payment);

            }
            user.Balance += (amount / 100);
            int result = await context.SaveChangesAsync(ct);
            if(result > 0)
            {
                return (true, "");
            }
            throw new Exception("Failed Adding funds");
        }
        public async Task<List<Transaction>> GetPortfolioTransactions(int portfolioId, string email, CancellationToken ct)
        {
            var user = await context.Users.Include(u => u.Portfolios).ThenInclude(p => p.Transactions).ThenInclude(t => t.Asset)
                .FirstOrDefaultAsync(u => u.Email == email, ct) ?? throw new Exception("User does not exist");
            var portfolio = user.Portfolios.FirstOrDefault(p => p.Id == portfolioId) ?? throw new Exception("Portfolio not found");
            return portfolio.Transactions;
        }
        public async Task<User> EditProfile(string oldEmail, string email, string fullName, string address, string profilePictureUrl, CancellationToken ct)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == oldEmail, ct) ?? throw new Exception("User does not exist");
            if(email != oldEmail)
            {
                var otherUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
                if(otherUser != null)
                {
                    throw new Exception("Email already in use!");
                }
            }
            user.Email = email;
            user.FullName = fullName;
            user.Address = address;
            if (!string.IsNullOrEmpty(profilePictureUrl))
            {
                user.ProfilePictureUrl = profilePictureUrl;
            }
            await context.SaveChangesAsync(ct);
            return user;
        }
        public async Task<List<User>> GetUsersFiltered(string? search, Roles? role, bool? enabled, CancellationToken ct)
        {
            var query = context.Users.Include(u => u.Portfolios).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (enabled.HasValue)
            {
                query = query.Where(u => u.Enabled == enabled.Value);
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync(ct);
        }
        public async Task<User> ToggleUserStatus(int userId, string adminEmail, CancellationToken ct)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new Exception("User not found.");

            if (user.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Cannot disable your own administrator account.");
            }

            user.Enabled = !user.Enabled;
            context.Users.Update(user);
            int result = await context.SaveChangesAsync(ct);
            if (result <= 0)
            {
                throw new Exception("Failed to update user status.");
            }
            return user;
        }
        public async Task<List<Transaction>> GetAllTransactions(string? search, TransactionType? type, CancellationToken ct)
        {
            var query = context.Transactions
                .Include(t => t.Asset)
                .Include(t => t.Portfolio)
                .ThenInclude(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(t => t.Asset.Ticker.ToLower().Contains(term)
                    || t.Asset.Name.ToLower().Contains(term)
                    || t.Portfolio.Name.ToLower().Contains(term)
                    || t.Portfolio.User.Email.ToLower().Contains(term)
                    || t.Portfolio.User.FullName.ToLower().Contains(term));
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
        }
        public async Task<(int TotalUsers, int TotalTraders, int TotalAdmins, int TotalPortfolios, int TotalAssets, int EnabledAssets, int DisabledAssets, int TotalTransactions, int BuyTransactionsCount, int SellTransactionsCount, decimal TotalTradingVolume, decimal TotalUserBalances, decimal TotalDepositedAmount)> GetPlatformMetrics(CancellationToken ct)
        {
            int totalUsers = await context.Users.CountAsync(ct);
            int totalTraders = await context.Users.CountAsync(u => u.Role == Roles.User, ct);
            int totalAdmins = await context.Users.CountAsync(u => u.Role == Roles.Admin, ct);
            int totalPortfolios = await context.Portfolios.CountAsync(ct);
            int totalAssets = await context.Assets.CountAsync(ct);
            int enabledAssets = await context.Assets.CountAsync(a => a.Enabled, ct);
            int disabledAssets = await context.Assets.CountAsync(a => !a.Enabled, ct);
            int totalTransactions = await context.Transactions.CountAsync(ct);
            int buyCount = await context.Transactions.CountAsync(t => t.Type == TransactionType.Buy, ct);
            int sellCount = await context.Transactions.CountAsync(t => t.Type == TransactionType.Sell, ct);
            decimal volume = totalTransactions > 0
                ? await context.Transactions.SumAsync(t => t.Quantity * t.PriceAtPurchase, ct)
                : 0m;
            decimal totalBalances = totalUsers > 0
                ? await context.Users.SumAsync(u => u.Balance, ct)
                : 0m;
            long? depositCents = await context.PaymentTraces
                .Where(p => p.Status == "succeeded")
                .SumAsync(p => (long?)p.Amount, ct);
            decimal totalDeposited = (depositCents ?? 0L) / 100m;

            return (totalUsers, totalTraders, totalAdmins, totalPortfolios, totalAssets, enabledAssets, disabledAssets, totalTransactions, buyCount, sellCount, volume, totalBalances, totalDeposited);
        }
        public async Task<List<Transaction>> GetRecentTransactions(int count, CancellationToken ct)
        {
            return await context.Transactions
                .Include(t => t.Asset)
                .Include(t => t.Portfolio)
                .ThenInclude(p => p.User)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
        }
        public async Task<List<User>> GetRecentUsers(int count, CancellationToken ct)
        {
            return await context.Users
                .Include(u => u.Portfolios)
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
        }
    }
}
