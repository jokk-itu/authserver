using Application.Validation;

namespace Infrastructure.Services.Abstract;
public interface IClientService
{
  Task<BaseValidationResult> ValidateRedirectAuthorization(
    string clientId,
    string? redirectUri,
    string state,
    CancellationToken cancellationToken);

  Task<bool> IsConsentValid(
    string clientId,
    string userId,
    string scope,
    CancellationToken cancellationToken);

  Task<BaseValidationResult> ValidateClientAuthorization(
    string scope,
    string clientId,
    CancellationToken cancellationToken);
}
