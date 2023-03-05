using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators;
public class OpenIdScopeValidator : IBaseValidator<ICollection<string>>
{
  private readonly IdentityContext _identityContext;

  public OpenIdScopeValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateAsync(ICollection<string> value, CancellationToken cancellationToken = default)
  {
    if (!value.Contains(ScopeConstants.OpenId))
    {
      return new BaseValidationResult(ErrorCode.InvalidScope, $"{ScopeConstants.OpenId} is required");
    }

    foreach (var scope in value)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope, cancellationToken: cancellationToken))
      {
        return new BaseValidationResult(ErrorCode.InvalidScope, $"{scope} is invalid");
      }
    }

    return new BaseValidationResult();
  }
}