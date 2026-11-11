using FluentValidation;
using Hi_Trade.DAL.Entities;
using Hi_Trade.Models.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        protected static string GetUserEmailFromToken(string token)
        {
            string? email = GetClaimFromToken(token, JwtRegisteredClaimNames.Sub, "sub");
            if (email == null)
            {
                throw new Exception("Email not found in JWT token");
            }
            return email;
        }

        protected static Roles GetUserRoleFromToken(string token)
        {
            string? roleValue = GetClaimFromToken(token, ClaimTypes.Role, "role");
            if (!Enum.TryParse<Roles>(roleValue, ignoreCase: true, out var role))
            {
                throw new Exception("Role not found in JWT token");
            }
            return role;
        }

        private static string? GetClaimFromToken(string token, params string[] claimTypes)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            return jwtToken.Claims.FirstOrDefault(c => claimTypes.Contains(c.Type))?.Value;
        }
    }
}
