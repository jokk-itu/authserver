using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Services.Abstract;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;
public class ScopeService : IScopeService
{
  private readonly IdentityContext _identityContext;

  public ScopeService(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateScope(string scope, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(scope)
        || !scope.Split(' ').Contains(ScopeConstants.OpenId))
    {
      return new BaseValidationResult(
        ErrorCode.InvalidScope,
        "scope does not contain openid");
    }

    var scopes = scope.Split(' ');
    var scopeAmount = await _identityContext
      .Set<Scope>()
      .Where(x => scopes.Any(y => y == x.Name))
      .CountAsync(cancellationToken: cancellationToken);

    if (scopes.Length != scopeAmount)
    {
      return new BaseValidationResult(
        ErrorCode.InvalidScope,
        "scope is invalid");
    }

    return new BaseValidationResult();
  }
}
