using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Common
{
    public class BaseResult<T> where T : class
    {

        public T? Model { get; set; }
        /// <summary>
        /// Message return by the service
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Type of result
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.Success;
    }
}
