using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.EndSession;
public class EndSessionValidator : IValidator<EndSessionCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public EndSessionValidator(
    IdentityContext identityContext,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(EndSessionCommand value, CancellationToken cancellationToken = default)
  {
    // TODO Ignore the exp claim
    var token = _tokenDecoder.DecodeSignedToken(value.IdTokenHint);
    if (token is null)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "id_token_hint is invalid", HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.ClientId))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "client_id is required", HttpStatusCode.BadRequest);
    }

    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.Aud).Value;
    if (value.ClientId != clientId)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "audience in id_token_hint does not match client_id", HttpStatusCode.BadRequest);
    }

    if (!string.IsNullOrWhiteSpace(value.PostLogoutRedirectUri))
    {
      var isValidRedirectUri = await _identityContext
        .Set<Client>()
        .Where(x => x.Id == value.ClientId)
        .SelectMany(x => x.RedirectUris)
        .Where(x => x.Type == RedirectUriType.PostLogoutRedirectUri)
        .Where(x => x.Uri == value.PostLogoutRedirectUri)
        .AnyAsync(cancellationToken: cancellationToken);

      if (!isValidRedirectUri)
      {
        return new ValidationResult(ErrorCode.UnauthorizedClient,
          "post_logout_redirect_uri is unauthorized for client", HttpStatusCode.BadRequest);
      }

      if (string.IsNullOrWhiteSpace(value.State))
      {
        return new ValidationResult(ErrorCode.InvalidRequest, "state is required when post_logout_redirect_uri is set", HttpStatusCode.BadRequest);
      }
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}