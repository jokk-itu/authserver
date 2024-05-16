using Domain;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.Abstract;

public abstract class AuthorizeHandler
{
  private readonly IdentityContext _identityContext;
  private readonly IAuthorizationGrantService _authorizationGrantService;

  protected AuthorizeHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService)
  {
    _identityContext = identityContext;
    _authorizationGrantService = authorizationGrantService;
  }

  protected async Task<string> CreateGrant(AuthorizeRequest request, string userId, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(c => c.RedirectUris)
      .SingleAsync(c => c.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<User>()
      .Where(u => u.Id == userId)
      .SelectMany(u => u.Sessions)
      .Where(s => !s.IsRevoked)
      .SingleAsync(cancellationToken: cancellationToken);

    var maxAge = string.IsNullOrWhiteSpace(request.MaxAge) ? client.DefaultMaxAge : long.Parse(request.MaxAge);
    var result = await _authorizationGrantService.CreateAuthorizationGrant(
      new CreateAuthorizationGrantArguments
      {
        Client = client,
        Session = session,
        CodeChallenge = request.CodeChallenge,
        CodeChallengeMethod = request.CodeChallengeMethod,
        Nonce = request.Nonce,
        Scope = request.Scope,
        RedirectUri = request.RedirectUri,
        MaxAge = maxAge
      }, cancellationToken);

    return result.Code;
  }

  protected async Task<string> UpdateGrant(
    AuthorizeRequest request, AuthorizationCodeGrant authorizationGrant, CancellationToken cancellationToken)
  {
    var result = await _authorizationGrantService.UpdateAuthorizationGrant(
      new UpdateAuthorizationGrantArguments
      {
        AuthorizationCodeGrant = authorizationGrant,
        Nonce = request.Nonce,
        Scope = request.Scope,
        CodeChallenge = request.CodeChallenge,
        CodeChallengeMethod = request.CodeChallengeMethod,
        RedirectUri = request.RedirectUri
      }, cancellationToken);

    return result.Code;
  }
}