using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginValidator : IValidator<SilentLoginQuery>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public SilentLoginValidator(
    IdentityContext identityContext,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(SilentLoginQuery value, CancellationToken cancellationToken = default)
  {
    var token = _tokenDecoder.DecodeSignedToken(value.IdTokenHint);
    if (token is null)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "id_token_hint is invalid", HttpStatusCode.OK);
    }

    var sessionId = long.Parse(token.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value);

    var isSessionValid = await _identityContext
      .Set<Session>()
      .Where(x => x.Id == sessionId)
      .Where(Session.IsValid)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isSessionValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is not valid", HttpStatusCode.OK);
    }

    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    if (value.ClientId != clientId)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "client_id does not match client_id in id_token_hint", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}