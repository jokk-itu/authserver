using Application;
using Application.Validation;
using Domain;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;
public class ConsentGrantService : IConsentGrantService
{
  private readonly IdentityContext _identityContext;

  public ConsentGrantService(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateConsentedScopes(
    string userId, string clientId, string scope, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(scope))
    {
      return new BaseValidationResult(
        ErrorCode.InvalidRequest,
        "scope is invalid");
    }

    var scopes = scope.Split(' ');
    var consentedScopes = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == userId)
      .Where(x => x.Client.Id == clientId)
      .SelectMany(x => x.ConsentedScopes)
      .Where(x => scopes.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);
    
    if (!Array.TrueForAll(scopes, s => consentedScopes.Exists(y => y.Name == s)))
    {
      return new BaseValidationResult(ErrorCode.ConsentRequired,
        "consent is required");
    }

    return new BaseValidationResult();
  }
}
