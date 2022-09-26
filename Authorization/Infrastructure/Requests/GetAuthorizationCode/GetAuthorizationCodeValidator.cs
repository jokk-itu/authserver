using System.Net;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetAuthorizationCode;
public class GetAuthorizationCodeValidator : IValidator<GetAuthorizationCodeQuery>
{
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;

  public GetAuthorizationCodeValidator(IdentityContext identityContext, UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _userManager = userManager;
  }

  public async Task<ValidationResult> IsValidAsync(GetAuthorizationCodeQuery value)
  {
    if (await IsClientIdInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "client_id is invalid", HttpStatusCode.BadRequest);

    if (IsRedirectUriInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "redirect_uri is invalid", HttpStatusCode.BadRequest);

    if (IsStateInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "state is invalid", HttpStatusCode.BadRequest);

    if(await IsClientUnauthorized(value))
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.Redirect);

    if (IsResponseTypeInvalid(value))
      return new ValidationResult(ErrorCode.UnsupportedResponseType, "response_type must be code", HttpStatusCode.Redirect);

    if (IsCodeChallengeInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge is invalid", HttpStatusCode.Redirect);

    if(IsCodeChallengeMethodInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code_challenge_method is invalid", HttpStatusCode.Redirect);

    if(await IsNonceInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "nonce is invalid", HttpStatusCode.Redirect);

    if(await IsScopesInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "scope is invalid", HttpStatusCode.Redirect);

    if (await IsUserInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "user is invalid", HttpStatusCode.Redirect);
    
    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientIdInvalidAsync(GetAuthorizationCodeQuery query)
  {
    if (string.IsNullOrWhiteSpace(query.ClientId))
      return true;

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == query.ClientId);

    return client is null;
  }

  private static bool IsRedirectUriInvalid(GetAuthorizationCodeQuery query)
  {
    return string.IsNullOrWhiteSpace(query.RedirectUri);
  }

  private async Task<bool> IsClientUnauthorized(GetAuthorizationCodeQuery query)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Grants)
      .Include(x => x.RedirectUris)
      .Include(x => x.Scopes)
      .SingleAsync(x => x.Id == query.ClientId);

    if (client.RedirectUris.All(x => x.Uri != query.RedirectUri))
      return true;

    if (client.Grants.All(x => x.Name != GrantType.AuthorizationCode))
      return true;

    return query.Scopes.All(x => client.Scopes.Any(y => y.Name == x));
  }

  private static bool IsResponseTypeInvalid(GetAuthorizationCodeQuery query)
  {
    return query.ResponseType != ResponseTypeConstants.Code;
  }

  private static bool IsCodeChallengeInvalid(GetAuthorizationCodeQuery query)
  {
    if (string.IsNullOrWhiteSpace(query.CodeChallenge))
      return true;

    return !Regex.IsMatch(query.CodeChallenge, "^[0-9a-zA-Z-_~.]{43,128}$");
  }

  private static bool IsCodeChallengeMethodInvalid(GetAuthorizationCodeQuery query)
  {
    return query.CodeChallengeMethod != CodeChallengeMethodConstants.S256;
  }

  private static bool IsStateInvalid(GetAuthorizationCodeQuery query)
  {
    return string.IsNullOrWhiteSpace(query.State);
  }

  private async Task<bool> IsNonceInvalidAsync(GetAuthorizationCodeQuery query)
  {
    if (string.IsNullOrWhiteSpace(query.Nonce))
      return true;

    return await _identityContext
      .Set<Nonce>()
      .AnyAsync(x => x.Value == query.Nonce);
  }

  private async Task<bool> IsScopesInvalidAsync(GetAuthorizationCodeQuery query)
  {
    if (query.Scopes.Any(x => x == ScopeConstants.OpenId))
      return true;

    foreach (var scope in query.Scopes)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope))
        return true;
    }

    return false;
  }

  private async Task<bool> IsUserInvalid(GetAuthorizationCodeQuery query)
  {
    if (string.IsNullOrWhiteSpace(query.Username) || string.IsNullOrWhiteSpace(query.Password))
      return true;

    var user = await _userManager.FindByNameAsync(query.Username);
    if (user is null)
      return true;

    return !await _userManager.CheckPasswordAsync(user, query.Password);
  }
}
