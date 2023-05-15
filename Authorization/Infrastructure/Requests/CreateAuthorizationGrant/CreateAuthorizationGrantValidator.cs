using System.Net;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantValidator : IValidator<CreateAuthorizationGrantCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateAuthorizationGrantValidator(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(CreateAuthorizationGrantCommand value, CancellationToken cancellationToken = default)
  {
    var query = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == value.ClientId)
      .Select(x => new
      {
        Client = x,
        IsRedirectUriAuthorized = x.RedirectUris.Any(y => y.Uri == value.RedirectUri),
        IsGrantTypeAuthorized = x.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode),
        x.Scopes,
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
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

    var scope = value.Scope?.Split(' ') ?? Array.Empty<string>();
    var isScopeAuthorized = !scope.Except(query.Scopes.Select(x => x.Name)).Any();
    if (!scope.Contains(ScopeConstants.OpenId) || !isScopeAuthorized)
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.OK);
    }

    if (!query.IsGrantTypeAuthorized || !query.IsRedirectUriAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.OK);
    }

    if (value.ResponseType != ResponseTypeConstants.Code)
    {
      return new ValidationResult(ErrorCode.UnsupportedResponseType, "response_type must be code", HttpStatusCode.OK);
    }

    if (string.IsNullOrWhiteSpace(value.CodeChallenge) ||
        !Regex.IsMatch(
          value.CodeChallenge,
          @"^[0-9a-zA-Z-_~.]{43,128}$",
          RegexOptions.None,
          TimeSpan.FromSeconds(1)))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge is invalid", HttpStatusCode.OK);
    }

    if (value.CodeChallengeMethod != CodeChallengeMethodConstants.S256)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge_method must be S256", HttpStatusCode.OK);
    }

    var isDuplicateNonce = await _identityContext
      .Set<Nonce>()
      .AnyAsync(x => x.Value == value.Nonce, cancellationToken: cancellationToken);

    if (isDuplicateNonce)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "nonce is invalid", HttpStatusCode.OK);
    }

    if (!string.IsNullOrWhiteSpace(value.MaxAge)
        && !(long.TryParse(value.MaxAge, out var maxAge)
        && maxAge > -1))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "max_age is invalid", HttpStatusCode.OK);;
    }
    
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedScopes)
      .Where(x => x.Client.Id == value.ClientId)
      .Where(x=> x.User.Id == value.UserId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var hasScopeConsent = !scope.Except(consentGrant?.ConsentedScopes.Select(x => x.Name) ?? Array.Empty<string>()).Any();

    if (consentGrant is null || !hasScopeConsent)
    {
      return new ValidationResult(ErrorCode.ConsentRequired, "consent is required", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}