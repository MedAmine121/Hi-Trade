using Hi_Trade.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class PriceBackgroundService(IServiceProvider serviceProvider, ILogger<PriceBackgroundService> logger) : BackgroundService
    {
        public static int RetryCount { get; private set; } = 0;
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    try
                    {
                        var hiTradeBLL = scope.ServiceProvider.GetRequiredService<IHiTradeBLL>();
                        logger.LogInformation("Updating asset prices...");
                        await hiTradeBLL.UpdateAssetPrices(ct);
                        RetryCount = 0;
                        logger.LogInformation("Asset prices updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while updating asset prices");
                        RetryCount++;
                        await Task.Delay(TimeSpan.FromMinutes(RetryCount * 2), ct);
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), ct);
            }
        }
    }
}
