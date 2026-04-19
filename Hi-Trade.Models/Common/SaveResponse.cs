using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Common
{
    public class SaveResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
