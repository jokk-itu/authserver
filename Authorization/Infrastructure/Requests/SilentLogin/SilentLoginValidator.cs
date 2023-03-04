using System.Net;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginValidator : IValidator<SilentLoginCommand>
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

  public async Task<ValidationResult> ValidateAsync(SilentLoginCommand value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.ClientId))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "client_id is invalid", HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.RedirectUri))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "redirect_uri is invalid", HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.State))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "state is invalid", HttpStatusCode.BadRequest);
    }

    // TODO do not validate time claims only the signing validation is done,
    // TODO it is only to be used against the session it contains
    var token = _tokenDecoder.DecodeSignedToken(value.IdTokenHint);
    if (token is null)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "id_token_hint is invalid", HttpStatusCode.OK);
    }

    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    if (value.ClientId != clientId)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "client_id does not match client_id in id_token_hint", HttpStatusCode.OK);
    }

    var scopes = value.Scope?.Split(' ');
    if (scopes is null || !scopes.Contains(ScopeConstants.OpenId))
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope does not contain openid", HttpStatusCode.OK);
    }

    var scopeAmount = await _identityContext
      .Set<Scope>()
      .Where(x => scopes.Any(y => y == x.Name))
      .CountAsync(cancellationToken: cancellationToken);

    if (scopes.Length != scopeAmount)
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.OK);
    }

    if (value.ResponseType != ResponseTypeConstants.Code)
    {
      return new ValidationResult(ErrorCode.UnsupportedResponseType, "response_type must be code", HttpStatusCode.OK);
    }

    if (value.CodeChallengeMethod != CodeChallengeMethodConstants.S256)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge_method must be S256", HttpStatusCode.OK);
    }

    if (string.IsNullOrWhiteSpace(value.Nonce))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "nonce is invalid", HttpStatusCode.OK);
    }

    if (string.IsNullOrWhiteSpace(value.CodeChallenge) || !Regex.IsMatch(value.CodeChallenge, @"^[0-9a-zA-Z-_~.]{43,128}$"))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge is invalid", HttpStatusCode.OK);
    }

    var authorizedClientQuery = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == clientId)
      .Select(x => new
      {
        IsGrantTypeAuthorized = x.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode),
        IsRedirectUriAuthorized = x.RedirectUris.Any(y => y.Uri == value.RedirectUri),
        AmountOfScopes = x.Scopes.Count(y => scopes.Any(z => z == y.Name))
      })
      .SingleAsync(cancellationToken: cancellationToken);

    if (!authorizedClientQuery.IsGrantTypeAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient,
        "client is unauthorized for authorization_code grant type", HttpStatusCode.OK);
    }

    if (!authorizedClientQuery.IsRedirectUriAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient,
        "client is unauthorized for redirect_uri", HttpStatusCode.OK);
    }

    if (authorizedClientQuery.AmountOfScopes != scopes.Length)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient,
        "client is unauthorized for scope", HttpStatusCode.OK);
    }

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;

    var consentedScopes = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == userId)
      .Where(x => x.Client.Id == clientId)
      .SelectMany(x => x.ConsentedScopes)
      .Where(x => scopes.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);
     
    if (!scopes.All(x => consentedScopes.Any(y => y.Name == x)))
    {
      return new ValidationResult(ErrorCode.ConsentRequired, "consent is required", HttpStatusCode.OK);
    }

    var isAuthorizationGrantValid = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isAuthorizationGrantValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "authorization_grant is not valid", HttpStatusCode.OK);
    }

    var sessionId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    var isSessionValid = await _identityContext
      .Set<Session>()
      .Where(x => x.Id == sessionId)
      .Where(x => !x.IsRevoked)
      .AnyAsync(cancellationToken: cancellationToken);
    
    if (!isSessionValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is not valid", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}