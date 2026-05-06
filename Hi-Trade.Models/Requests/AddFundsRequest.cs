using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class AddFundsRequest
    {
        public long Amount { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
    }
    public class AddFundsValidator: AbstractValidator<AddFundsRequest>
    {
        public AddFundsValidator()
        {
            RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Add funds amount should be greater than 0");
            RuleFor(r => r.Email).NotNull().WithMessage("Issue with the session email");
        }
    }
}
