using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;
using System.Threading.Tasks;
using FluentValidation;
using Hi_Trade.DAL;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Responses;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.Models.Common;
using Hi_Trade.Common;
using Microsoft.Extensions.Configuration;
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
        public async Task<UserDTO?> LoginUser(LoginUserRequest request, CancellationToken ct)
        {
            var user = await hiTradeDAL.LoginUser(request.Email, ct);
            if(user == null)
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
            foreach (var asset in assets)
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
                if(priceData == null)
                {
                    if(retries < 5)
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
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
