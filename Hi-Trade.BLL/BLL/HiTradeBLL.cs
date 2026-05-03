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
        public async Task<List<PortfolioDTO>> GetUserPortfolios(string email, CancellationToken ct)
        {
            var portfolios = await hiTradeDAL.GetPortfolios(email, ct);
            List<PortfolioDTO> portfolioDTOs = new();
            foreach(Portfolio portfolio in portfolios)
            {
                PortfolioDTO portfolioDTO = new();
                portfolioDTO.Name = portfolio.Name;
                portfolioDTO.Id = portfolio.Id;
                portfolioDTO.CurrentValue = portfolio.Positions.Sum(p => p.Quantity * p.Asset.CurrentPrice);
                portfolioDTO.GainLoss = portfolio.TotalRealizedGainLoss + portfolioDTO.CurrentValue - portfolio.Positions.Sum(p => p.Quantity * p.AveragePrice);
                portfolioDTO.Performance = portfolio.NetInvested != 0 ? (portfolioDTO.GainLoss / portfolio.NetInvested) * 100 : 0;
                portfolioDTO.Positions = portfolio.Positions.Select(p => new PositionDTO()
                {
                    AveragePrice = p.AveragePrice,
                    Quantity = p.Quantity,
                    Asset = new AssetDTO()
                    {
                        Ticker = p.Asset.Ticker,
                        Name = p.Asset.Name,
                        CurrentPrice = p.Asset.CurrentPrice,
                        Id = p.Id
                    }
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
        public async Task<List<AssetDTO>> GetAllEnabledAssets(CancellationToken ct)
        {
            return (await hiTradeDAL.GetAllAssets(ct)).Where(a => a.Enabled).Select(a => new AssetDTO()
            {
                Name = a.Name,
                Ticker = a.Ticker,
                CurrentPrice = a.CurrentPrice,
                Id = a.Id
            }).ToList();
        }
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
