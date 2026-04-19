using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class CreateAssetRequest
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
