using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Common
{
    public class RedisOptions
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
