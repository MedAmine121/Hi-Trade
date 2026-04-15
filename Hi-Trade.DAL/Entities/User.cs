using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public List<Portfolio> Portfolios { get; set; } = new();
    }
}
