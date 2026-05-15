using FluentValidation;

namespace Hi_Trade.Models.Requests
{
    public class GetPortfolioTransactionsRequest
    {
        public int PortfolioId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
    public class GetPortfolioTransactionsValidator : AbstractValidator<GetPortfolioTransactionsRequest>
    {
        public GetPortfolioTransactionsValidator()
        {
            RuleFor(user => user.PortfolioId).GreaterThan(0).WithMessage("Portfolio not found!");
            RuleFor(user => user.Email).EmailAddress().WithMessage("Email not found.");
        }
    }
}
