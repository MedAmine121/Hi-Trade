using FluentValidation;

namespace Hi_Trade.Models.Requests
{
    public class ResendVerificationEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResendVerificationEmailValidator : AbstractValidator<ResendVerificationEmailRequest>
    {
        public ResendVerificationEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
