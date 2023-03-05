using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;
public class CreateOrUpdateConsentGrantValidator : IValidator<CreateOrUpdateConsentGrantCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateOrUpdateConsentGrantValidator(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(CreateOrUpdateConsentGrantCommand value, CancellationToken cancellationToken = default)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Scopes)
      .SingleOrDefaultAsync(x => x.Id == value.ClientId, cancellationToken: cancellationToken);

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);
    }

    var isClientAuthorized = value.ConsentedScopes.All(x => client.Scopes.Any(y => y.Name == x));

    if (!isClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    var isUserValid = await _identityContext
      .Set<User>()
      .Where(x => x.Id == value.UserId)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isUserValid)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "user is invalid", HttpStatusCode.BadRequest);
    }

    if (await AreClaimsInvalid(value, cancellationToken))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "consented claim is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> AreClaimsInvalid(CreateOrUpdateConsentGrantCommand command,
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
