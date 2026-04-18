using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Common
{
    public enum ResultType
    {
        Success = 0,
        Fail = 1,
        Error = 2,
        Conflict = 3,
        Created = 4,
        Unauthorized = 5,
        ExpectationFailed = 6,
        PartialSuccess = 7,
        SuccessWithWarning = 8
    }
}
