using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;
public class CreateOrUpdateConsentGrantValidator : IValidator<CreateOrUpdateConsentGrantCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateOrUpdateConsentGrantValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(CreateOrUpdateConsentGrantCommand value, CancellationToken cancellationToken = default)
  {
   
    if (await IsUserInvalid(value, cancellationToken))
      return new ValidationResult(ErrorCode.InvalidRequest, "user is invalid", HttpStatusCode.BadRequest);

    
    if (await IsClientInvalid(value, cancellationToken))
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);

    
    if (await AreScopesInvalid(value, cancellationToken))
      return new ValidationResult(ErrorCode.InvalidRequest, "consented scope is invalid", HttpStatusCode.BadRequest);

    
    if (await AreClaimsInvalid(value, cancellationToken))
      return new ValidationResult(ErrorCode.InvalidRequest, "consented claim is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsUserInvalid(CreateOrUpdateConsentGrantCommand command, CancellationToken cancellationToken)
  {
    return !await _identityContext
      .Set<User>()
      .AnyAsync(x => x.Id == command.UserId, cancellationToken: cancellationToken);
  }

  private async Task<bool> IsClientInvalid(CreateOrUpdateConsentGrantCommand command,
    CancellationToken cancellationToken)
  {
    return !await _identityContext
      .Set<Client>()
      .AnyAsync(x => x.Id == command.ClientId, cancellationToken: cancellationToken);
  }

  private async Task<bool> AreScopesInvalid(CreateOrUpdateConsentGrantCommand command,
    CancellationToken cancellationToken)
  {
    foreach (var scope in command.ConsentedScopes)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope, cancellationToken: cancellationToken))
        return true;
    }

    return false;
  }

  private async Task<bool> AreClaimsInvalid(CreateOrUpdateConsentGrantCommand command,
    CancellationToken cancellationToken)
  {
    foreach (var claim in command.ConsentedClaims)
    {
      if (!await _identityContext.Set<Claim>().AnyAsync(x => x.Name == claim, cancellationToken: cancellationToken))
        return true;
    }

    return false;
  }
}
