using Hi_Trade.BLL.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class PriceBackgroundService(IHiTradeBLL hiTradeBLL, ILogger<PriceBackgroundService> logger) : BackgroundService
    {
        public static bool IsBusy { get; set; } = false;
        protected override Task ExecuteAsync(CancellationToken ct)
        {
            IsBusy = true;
            try
            {

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the price background service");
            }
            IsBusy = false;
            return Task.CompletedTask;
        }
    }
}
