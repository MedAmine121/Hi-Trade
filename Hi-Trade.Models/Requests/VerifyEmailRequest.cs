using FluentValidation;

namespace Hi_Trade.Models.Requests
{
    public class VerifyEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class VerifyEmailValidator : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code is required.");
        }
    }
}
