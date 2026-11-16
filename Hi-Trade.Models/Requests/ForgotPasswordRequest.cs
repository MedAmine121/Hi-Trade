using FluentValidation;

namespace Hi_Trade.Models.Requests
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
