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
    }
}
