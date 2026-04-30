using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class CreatePortfolioRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
    }
    public class CreatePortfolioValidator : AbstractValidator<CreatePortfolioRequest>
    {
        public CreatePortfolioValidator()
        {
            RuleFor(user => user.Name).NotEmpty().WithMessage("Invalid Name.");
            RuleFor(user => user.Name).NotEmpty().WithMessage("Email not found.");
        }
    }
}
