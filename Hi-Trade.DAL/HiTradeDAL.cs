using Hi_Trade.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL
{
    public class HiTradeDAL(HiTradeContext context) : IHiTradeDAL
    {
        public async Task<User> CreateUser(string email, string password, string fullName, string address, CancellationToken ct)
        {
            User user = new()
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Address = address
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
    }
}
