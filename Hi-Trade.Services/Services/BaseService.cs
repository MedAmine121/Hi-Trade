using FluentValidation;
using Hi_Trade.Models.Common;

namespace Hi_Trade.Services.Services
{
    public class BaseService(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public virtual async Task<Response?> Validate<Request, Response>(Func<Request, CancellationToken, Task<Response?>> invoke, Request request, CancellationToken ct) where Response : class
        {
            var validator = _serviceProvider.GetService(typeof(IValidator<Request>)) as IValidator<Request>;
            if(validator != null)
            {
                var validationResult = await validator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }
            return await invoke(request, ct);
        }
    }
}
