using FluentValidation;
using Hi_Trade.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(user => user.Email).EmailAddress().WithMessage("Invalid email address.");
            RuleFor(user => user.Password).MinimumLength(12).WithMessage("Password must be at least 12 characters long.");
            RuleFor(user => user.FullName).NotEmpty().WithMessage("Full Name is required.");
            RuleFor(user => user.Address).NotEmpty().WithMessage("Address is required.");
        }
    }
}
