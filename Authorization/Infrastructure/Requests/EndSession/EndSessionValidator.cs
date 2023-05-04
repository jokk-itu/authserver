using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Enums;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.EndSession;
public class EndSessionValidator : IValidator<EndSessionCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public EndSessionValidator(
    IdentityContext identityContext,
    IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(EndSessionCommand value, CancellationToken cancellationToken = default)
  {
    try
    {
      await _tokenDecoder.Decode(value.IdTokenHint, new StructuredTokenDecoderArguments
      {
        ClientId = value.ClientId,
        Audiences = new[] { value.ClientId },
        ValidateAudience = true,
        ValidateLifetime = false
      });
    }
    catch (Exception)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "id_token_hint is invalid", HttpStatusCode.BadRequest);
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