using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class SellAssetRequest
    {
        public int PositionId { get; set; } = 0;
        public int PortfolioId { get; set; } = 0;
        public decimal Quantity { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
    }
    public class SellAssetRequestValidator : AbstractValidator<SellAssetRequest>
    {
        public SellAssetRequestValidator()
        {
            RuleFor(asset => asset.PortfolioId).GreaterThan(0);
            RuleFor(asset => asset.PositionId).GreaterThan(0);
            RuleFor(asset => asset.Quantity).GreaterThan(0);
        }
    }
}
