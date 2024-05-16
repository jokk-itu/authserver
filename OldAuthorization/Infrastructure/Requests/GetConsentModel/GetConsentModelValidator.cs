using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetConsentModel;
public class GetConsentModelValidator : IValidator<GetConsentModelQuery>
{
  private readonly IdentityContext _identityContext;

  public GetConsentModelValidator(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(GetConsentModelQuery value, CancellationToken cancellationToken = default)
  {
    var requestedScopes = value.Scope.Split(' ');
    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => requestedScopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    if (requestedScopes.Length != scopes.Count)
    {
      return new ValidationResult(ErrorCode.InvalidRequest,
        "scope is invalid", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}