using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateConsentGrant;
public class CreateConsentGrantValidator : IValidator<CreateConsentGrantCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateConsentGrantValidator(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(CreateConsentGrantCommand value, CancellationToken cancellationToken = default)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Scopes)
      .SingleAsync(x => x.Id == value.ClientId, cancellationToken: cancellationToken);

    var isClientAuthorized = value.ConsentedScopes.All(x => client.Scopes.Any(y => y.Name == x));

    if (!isClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    if (await AreClaimsInvalid(value, cancellationToken))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "consented claim is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> AreClaimsInvalid(CreateConsentGrantCommand command,
    CancellationToken cancellationToken)
  {
    foreach (var claim in command.ConsentedClaims)
    {
      if (!await _identityContext.Set<Claim>().AnyAsync(x => x.Name == claim, cancellationToken: cancellationToken))
      {
        return true;
      }
    }

    return false;
  }
}
