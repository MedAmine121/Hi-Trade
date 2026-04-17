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
namespace Hi_Trade.BLL.BLL
{
    public class HiTradeBLL(IHiTradeDAL hiTradeDAL, IValidator<CreateUserRequest> createUserValidator) : IHiTradeBLL
    {
        public async Task<UserDTO> CreateUser(CreateUserRequest request, CancellationToken ct)
        {
            var validationResult = await createUserValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
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

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
