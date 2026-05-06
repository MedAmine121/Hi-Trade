using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Models.Requests
{
    public class ConfirmPaymentRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
    }
    public class ConfirmPaymentValidator : AbstractValidator<ConfirmPaymentRequest>
    {
        public ConfirmPaymentValidator()
        {
            RuleFor(c => c.Email).NotEmpty().WithMessage("Error fetching user");
            RuleFor(c => c.PaymentId).NotEmpty().WithMessage("PaymentId should not be null");
        }
    }
}
