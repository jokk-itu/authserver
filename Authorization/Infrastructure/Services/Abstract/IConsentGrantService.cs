using Application.Validation;

namespace Infrastructure.Services.Abstract;
public interface IConsentGrantService
{
  Task<BaseValidationResult> ValidateConsentedScopes(string userId, string clientId, string scope, CancellationToken cancellationToken);
}
