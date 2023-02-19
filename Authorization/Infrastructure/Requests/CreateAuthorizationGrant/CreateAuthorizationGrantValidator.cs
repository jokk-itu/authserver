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
    if (await IsClientIdInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "client_id is invalid", HttpStatusCode.BadRequest);

    if (IsRedirectUriInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "redirect_uri is invalid", HttpStatusCode.BadRequest);

    if (IsStateInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "state is invalid", HttpStatusCode.BadRequest);

    if (await IsClientUnauthorized(value))
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.OK);

    if (IsResponseTypeInvalid(value))
      return new ValidationResult(ErrorCode.UnsupportedResponseType, "response_type must be code", HttpStatusCode.OK);

    if (IsCodeChallengeInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge is invalid", HttpStatusCode.OK);

    if (IsCodeChallengeMethodInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge_method is invalid", HttpStatusCode.OK);

    if (await IsNonceInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "nonce is invalid", HttpStatusCode.OK);

    if (await IsScopeInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "scope is invalid", HttpStatusCode.OK);

    if (IsMaxAgeInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "max_age is invalid", HttpStatusCode.OK);

    if (await IsConsentGrantInvalid(value))
      return new ValidationResult(ErrorCode.ConsentRequired, "consent is required", HttpStatusCode.OK);
    
    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientIdInvalidAsync(CreateAuthorizationGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientId))
      return true;

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == command.ClientId);

    return client is null;
  }

  private static bool IsRedirectUriInvalid(CreateAuthorizationGrantCommand command)
  {
    return string.IsNullOrWhiteSpace(command.RedirectUri);
  }

  private async Task<bool> IsClientUnauthorized(CreateAuthorizationGrantCommand command)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.GrantTypes)
      .Include(x => x.RedirectUris)
      .Include(x => x.Scopes)
      .SingleAsync(x => x.Id == command.ClientId);

    if (client.RedirectUris.All(x => x.Uri != command.RedirectUri))
    {
      return true;
    }

    if (client.GrantTypes.All(x => x.Name != GrantTypeConstants.AuthorizationCode))
    {
      return true;
    }

    return command.Scope.Split(' ').All(x => client.Scopes.All(y => y.Name != x));
  }

  private static bool IsResponseTypeInvalid(CreateAuthorizationGrantCommand command)
  {
    return command.ResponseType != ResponseTypeConstants.Code;
  }

  private static bool IsCodeChallengeInvalid(CreateAuthorizationGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.CodeChallenge))
      return true;

    return !Regex.IsMatch(command.CodeChallenge, "^[0-9a-zA-Z-_~.]{43,128}$");
  }

  private static bool IsCodeChallengeMethodInvalid(CreateAuthorizationGrantCommand command)
  {
    return command.CodeChallengeMethod != CodeChallengeMethodConstants.S256;
  }

  private static bool IsStateInvalid(CreateAuthorizationGrantCommand command)
  {
    return string.IsNullOrWhiteSpace(command.State);
  }

  private async Task<bool> IsNonceInvalidAsync(CreateAuthorizationGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.Nonce))
    {
      return true;
    }

    return await _identityContext
      .Set<AuthorizationCodeGrant>()
      .AnyAsync(x => x.Nonce == command.Nonce);
  }

  private async Task<bool> IsScopeInvalidAsync(CreateAuthorizationGrantCommand command)
  {
    var scopes = command.Scope.Split(' ');
    if (scopes.All(x => x != ScopeConstants.OpenId))
    {
      return true;
    }

    foreach (var scope in scopes)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope))
      {
        return true;
      }
    }

    return false;
  }

  private async Task<bool> IsConsentGrantInvalid(CreateAuthorizationGrantCommand command)
  {
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedScopes)
      .Where(x => x.Client.Id == command.ClientId && x.User.Id == command.UserId)
      .SingleOrDefaultAsync();

    if (consentGrant is null)
    {
      return true;
    }

    if (consentGrant.ConsentedScopes.Count != command.Scope.Split(' ').Length)
    {
      return true;
    }

    return consentGrant.ConsentedScopes.Any(scope => !command.Scope.Split(' ').Contains(scope.Name));
  }

  private static bool IsMaxAgeInvalid(CreateAuthorizationGrantCommand command)
  {
    if (!string.IsNullOrWhiteSpace(command.MaxAge)
        && !long.TryParse(command.MaxAge, out var maxAge)
        && maxAge > -1)
    {
      return true;
    }

    return false;
  }
}