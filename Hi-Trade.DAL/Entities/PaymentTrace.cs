using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.Entities
{
    public class PaymentTrace : BaseEntity
    {
        public string PaymentId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public long Amount { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
    }
}
