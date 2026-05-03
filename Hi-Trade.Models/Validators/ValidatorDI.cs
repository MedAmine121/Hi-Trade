using FluentValidation;
using Hi_Trade.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Validators
{
    public static class ValidatorDI
    {
        public static IServiceCollection RegisterValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>()
                .AddScoped<IValidator<CreateAssetRequest>, CreateAssetValidator>()
                .AddScoped<IValidator<CreatePortfolioRequest>, CreatePortfolioValidator>()
                .AddScoped<IValidator<BuyAssetRequest>, BuyAssetRequestValidator>()
                .AddScoped<IValidator<SellAssetRequest>, SellAssetRequestValidator>();
            return services;
        }
    }
}
