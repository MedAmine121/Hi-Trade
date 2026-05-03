using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class BuyAssetRequest
    {
        public string Email { get; set; } = string.Empty;
        public int PortfolioId { get; set; } = 0;
        public int AssetId { get; set; } = 0;
        public decimal Quantity { get; set; } = 0;
    }
    public class BuyAssetRequestValidator : AbstractValidator<BuyAssetRequest>
    {
        public BuyAssetRequestValidator()
        {
            RuleFor(x => x.PortfolioId).GreaterThan(0);
            RuleFor(x => x.AssetId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
