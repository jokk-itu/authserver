using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantValidator : IValidator<RedeemRefreshTokenGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly IResourceService _resourceService;

  public RedeemRefreshTokenGrantValidator(
    IdentityContext identityContext,
    IStructuredTokenDecoder tokenDecoder,
    IResourceService resourceService)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
    _resourceService = resourceService;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.RefreshToken))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);
    }

    if (value.ClientAuthentications.Count != 1)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "multiple or none client authentication methods detected",
        HttpStatusCode.BadRequest);
    }

    var clientAuthentication = value.ClientAuthentications.Single();

    string? authorizationGrantId;
    if (value.RefreshToken.Split('.').Length == 3)
    {
      authorizationGrantId = await ValidateStructuredToken(clientAuthentication, value.RefreshToken, cancellationToken);
    }
    else
    {
      authorizationGrantId = await ValidateReferenceToken(value.RefreshToken, cancellationToken);
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
        IsClientIdValid = x.Client.Id == clientAuthentication.ClientId,
        ClientSecret = x.Client.Secret,
        IsClientAuthorized = x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.RefreshToken),
        IsSessionValid = !x.Session.IsRevoked,
        UserId = x.Session.User.Id
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "authorization grant is invalid", HttpStatusCode.BadRequest);
    }

    var isClientSecretValid = query.ClientSecret == null
                         || !string.IsNullOrWhiteSpace(clientAuthentication.ClientSecret)
                         && BCrypt.CheckPassword(clientAuthentication.ClientSecret, query.ClientSecret);

    if (!query.IsClientIdValid || !isClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    if (!query.IsSessionValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is invalid", HttpStatusCode.BadRequest);
    }

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == query.UserId)
      .Where(x => x.Client.Id == clientAuthentication.ClientId)
      .Include(x => x.ConsentedScopes)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (consentGrant is null)
    {
      return new ValidationResult(ErrorCode.ConsentRequired, "consent is required", HttpStatusCode.BadRequest);
    }

    if (!string.IsNullOrWhiteSpace(value.Scope)
        && value.Scope
          .Split(' ')
          .Except(consentGrant.ConsentedScopes.Select(x => x.Name))
          .Any())
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope exceeds consented scope", HttpStatusCode.BadRequest);
    }

    var scope = string.IsNullOrWhiteSpace(value.Scope) ? string.Join(' ', consentGrant.ConsentedScopes.Select(s => s.Name)) : value.Scope;
    var resourceValidation = await _resourceService.ValidateResources(value.Resource, scope);
    if (resourceValidation.IsError())
    {
      return new ValidationResult(
        resourceValidation.ErrorCode,
        resourceValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<string?> ValidateReferenceToken(string refreshToken, CancellationToken cancellationToken)
  {
    var authorizationGrantId = await _identityContext
      .Set<RefreshToken>()
      .Where(x => x.Reference == refreshToken)
      .Where(x => x.RevokedAt == null)
      .Where(x => x.ExpiresAt > DateTime.UtcNow)
      .Select(x => x.AuthorizationGrant.Id)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    return authorizationGrantId;
  }

  private async Task<string?> ValidateStructuredToken(ClientAuthentication clientAuthentication, string refreshToken, CancellationToken cancellationToken)
  {
    try
    {
      var token = await _tokenDecoder.Decode(refreshToken, new StructuredTokenDecoderArguments
      {
        ClientId = clientAuthentication.ClientId,
        Audiences = new[] { clientAuthentication.ClientId },
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
