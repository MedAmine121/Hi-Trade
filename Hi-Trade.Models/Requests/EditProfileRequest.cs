using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class EditProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string OldEmail { get; set; } = string.Empty;
    }
    public class EditProfileValidator : AbstractValidator<EditProfileRequest>
    {
        public EditProfileValidator()
        {
            RuleFor(e => e.Email).EmailAddress().WithMessage("Email address not valid");
            RuleFor(e => e.FullName).NotEmpty().WithMessage("Full Name is required");
            RuleFor(e => e.Address).NotEmpty().WithMessage("Address is required");
        }
    }
}
