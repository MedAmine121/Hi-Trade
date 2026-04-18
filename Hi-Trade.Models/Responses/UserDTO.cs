using Hi_Trade.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Responses
{
    public class UserDTO
    {
        public int Id { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public Roles Role { get; set; } = Roles.User;
        public string Token { get; set; } = string.Empty;
    }
}
