using Hi_Trade.BLL.BLL;
using Hi_Trade.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.BLL.DI
{
    public static class DIBLLExtension
    {
        public static IServiceCollection AddBLLServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenBLL, TokenBLL>()
                .AddScoped<IHiTradeBLL, HiTradeBLL>();
            return services;
        }
    }
}
