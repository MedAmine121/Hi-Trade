using FluentValidation;
using Hi_Trade.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Validators
{
    public class CreateAssetValidator : AbstractValidator<CreateAssetRequest>
    {
        public CreateAssetValidator()
        {
            RuleFor(asset => asset.Ticker).NotEmpty().WithMessage("Ticker is required.");
            RuleFor(asset => asset.Name).NotEmpty().WithMessage("Name is required.");
        }
    }
}
