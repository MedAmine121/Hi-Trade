
using Hi_Trade.DAL.Entities;

namespace Hi_Trade.DAL
{
    public interface IHiTradeDAL
    {
        Task<User> CreateUser(string email, string password, string fullName, string address, CancellationToken ct);
        Task<User?> LoginUser(string email, CancellationToken ct);
    }
}