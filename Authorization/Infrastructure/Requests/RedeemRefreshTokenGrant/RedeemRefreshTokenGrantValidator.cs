using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantValidator : IValidator<RedeemRefreshTokenGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public RedeemRefreshTokenGrantValidator(
    IdentityContext identityContext,
    IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.RefreshToken))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);
    }

    string? authorizationGrantId;
    if (value.RefreshToken.Split('.').Length == 3)
    {
      authorizationGrantId = await ValidateStructuredToken(value, cancellationToken);
    }
    else
    {
      authorizationGrantId = await ValidateReferenceToken(value, cancellationToken);
    }

    if (string.IsNullOrWhiteSpace(authorizationGrantId))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.RefreshToken)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant_type must be refresh_token", HttpStatusCode.BadRequest);
    }

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .Select(x => new
      {
        IsClientIdValid = x.Client.Id == value.ClientId,
        IsClientSecretValid = x.Client.Secret == value.ClientSecret,
        HasClientSecret = x.Client.Secret != null,
        IsClientAuthorized = x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.RefreshToken),
        IsSessionValid = !x.Session.IsRevoked
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "authorization grant is invalid", HttpStatusCode.BadRequest);
    }

    // TODO verify value.Scope that it is within the ConsentedGrant

    if (!query.IsClientIdValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_id is invalid", HttpStatusCode.BadRequest);
    }

    if (query.HasClientSecret && !query.IsClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_secret is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    if (!query.IsSessionValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<string?> ValidateReferenceToken(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken)
  {
    var authorizationGrantId = await _identityContext
      .Set<RefreshToken>()
      .Where(x => x.Reference == value.RefreshToken)
      .Where(x => x.RevokedAt == null)
      .Where(x => x.ExpiresAt > DateTime.UtcNow)
      .Select(x => x.AuthorizationGrant.Id)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    return authorizationGrantId;
  }

  private async Task<string?> ValidateStructuredToken(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken)
  {
    try
    {
      var token = await _tokenDecoder.Decode(value.RefreshToken, new StructuredTokenDecoderArguments
      {
        ClientId = value.ClientId,
        Audiences = new[] { value.ClientId },
        ValidateAudience = true,
        ValidateLifetime = true
      });
      var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
      var jti = Guid.Parse(token.Claims.Single(x => x.Type == ClaimNameConstants.Jti).Value);

      var isRevoked = await _identityContext
        .Set<RefreshToken>()
        .Where(x => x.Id == jti)
        .Where(x => x.RevokedAt != null)
        .AnyAsync(cancellationToken: cancellationToken);

      if (isRevoked)
      {
        return null;
      }

      return authorizationGrantId;
    }
    catch (Exception)
    {
      return null;
    }
  }
}
