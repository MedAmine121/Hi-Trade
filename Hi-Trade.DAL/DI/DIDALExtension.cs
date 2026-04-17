using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.DAL.DI
{
    public static class DIDALExtension
    {
        public static IServiceCollection AddDALServices(this IServiceCollection services)
        {
            services.AddScoped<IHiTradeDAL, HiTradeDAL>();
            return services;
        }
    }
}
