using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators;
public class ClaimValidator : IBaseValidator<ICollection<string>>
{
  private readonly IdentityContext _identityContext;

  public ClaimValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateAsync(ICollection<string> value, CancellationToken cancellationToken = default)
  {
    foreach (var claim in value)
    {
      if (!await _identityContext.Set<Claim>().AnyAsync(x => x.Name == claim, cancellationToken: cancellationToken))
        return new BaseValidationResult(ErrorCode.InvalidRequest, "claim is invalid");
    }

    return new BaseValidationResult();
  }
}
