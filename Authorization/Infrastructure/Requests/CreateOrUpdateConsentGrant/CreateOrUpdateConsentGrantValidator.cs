using System.Net;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;
public class CreateOrUpdateConsentGrantValidator : IValidator<CreateOrUpdateConsentGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeDecoder _codeDecoder;

  public CreateOrUpdateConsentGrantValidator(
    IdentityContext identityContext,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _codeDecoder = codeDecoder;
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

    var code = _codeDecoder.DecodeLoginCode(value.LoginCode);
    var isUserValid = await _identityContext
      .Set<User>()
      .Where(x => x.Id == code.UserId)
      .SelectMany(x => x.UserTokens)
      .Where(UserToken.IsActive)
      .Where(x => x.Value == value.LoginCode)
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
        return true;
    }

    return false;
  }
}
