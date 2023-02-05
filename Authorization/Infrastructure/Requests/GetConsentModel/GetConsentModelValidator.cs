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
    var isUserValid = await _identityContext
      .Set<User>()
      .AnyAsync(x => x.Id == value.UserId, cancellationToken: cancellationToken);

    if (!isUserValid)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "user_id is invalid", HttpStatusCode.OK);
    }

    var isClientValid = await _identityContext
      .Set<Client>()
      .AnyAsync(x => x.Id == value.ClientId, cancellationToken: cancellationToken);

    if (!isClientValid)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "client_id is invalid", HttpStatusCode.OK);
    }

    var requestedScopes = value.Scope.Split(' ');
    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => requestedScopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    if (requestedScopes.Length != scopes.Count)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "scope is invalid", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}