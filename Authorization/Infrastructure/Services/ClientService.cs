using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ClientService : IClientService
{
  private readonly IdentityContext _identityContext;

  public ClientService(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateRedirectAuthorization(
    string clientId,
    string? redirectUri,
    string state,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(state))
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "state is invalid");
    }

    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == clientId)
      .Include(x => x.RedirectUris)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (client is null)
    {
      return new BaseValidationResult(ErrorCode.InvalidClient, "client_id is invalid");
    }

    if (string.IsNullOrWhiteSpace(redirectUri)
        && client.RedirectUris.Count(x => x.Type == RedirectUriType.AuthorizeRedirectUri) != 1)
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "redirect_uri is missing");
    }

    if (!string.IsNullOrWhiteSpace(redirectUri)
        && !client.RedirectUris
          .Any(x => x.Uri == redirectUri && x.Type == RedirectUriType.AuthorizeRedirectUri))
    {
      return new BaseValidationResult(ErrorCode.UnauthorizedClient, "redirect_uri is unauthorized");
    }

    return new BaseValidationResult();
  }

  public async Task<bool> IsConsentValid(
    string clientId,
    string userId,
    string scope,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(scope))
    {
      return false;
    }

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedScopes)
      .Where(x => x.User.Id == userId)
      .Where(x => x.Client.Id == clientId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (consentGrant == null)
    {
      return false;
    }

    var scopes = scope.Split(' ');
    return !scopes.Except(consentGrant.ConsentedScopes.Select(x => x.Name)).Any();
  }

  public async Task<BaseValidationResult> ValidateClientAuthorization(string scope, string clientId,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(scope))
    {
      return new BaseValidationResult(
        ErrorCode.InvalidRequest,
        "scope is invalid");
    }

    var scopes = scope.Split(' ');
    var authorizedClientQuery = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == clientId)
      .Select(x => new
      {
        IsGrantTypeAuthorized = x.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode),
        AmountOfScopes = x.Scopes.Count(y => scopes.Any(z => z == y.Name))
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (authorizedClientQuery is null)
    {
      return new BaseValidationResult(
        ErrorCode.InvalidClient, "client is invalid");
    }

    if (!authorizedClientQuery.IsGrantTypeAuthorized)
    {
      return new BaseValidationResult(ErrorCode.UnauthorizedClient,
        "client is unauthorized for authorization_code grant type");
    }

    if (authorizedClientQuery.AmountOfScopes != scopes.Length)
    {
      return new BaseValidationResult(ErrorCode.UnauthorizedClient,
        "client is unauthorized for scope");
    }

    return new BaseValidationResult();
  }
}