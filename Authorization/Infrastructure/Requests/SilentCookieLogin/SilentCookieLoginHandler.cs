using Domain;
using MediatR;
using System.Net;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentCookieLogin;
public class SilentCookieLoginHandler : IRequestHandler<SilentCookieLoginCommand, SilentCookieLoginResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IAuthorizationGrantService _authorizationGrantService;
  
  public SilentCookieLoginHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService)
  {
    _identityContext = identityContext;
    _authorizationGrantService = authorizationGrantService;
  }

  public async Task<SilentCookieLoginResponse> Handle(SilentCookieLoginCommand request, CancellationToken cancellationToken)
  {
    var authorizationGrant = await _identityContext
      .Set<User>()
      .Where(u => u.Id == request.UserId)
      .SelectMany(u => u.Sessions)
      .Where(s => !s.IsRevoked)
      .SelectMany(s => s.AuthorizationCodeGrants)
      .Where(g => g.Client.Id == request.ClientId)
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .Include(g => g.Client)
      .ThenInclude(c => c.RedirectUris)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    string code;
    if (authorizationGrant is null)
    {
      code = await CreateGrant(request, cancellationToken);
    }
    else
    {
      code = await UpdateGrant(authorizationGrant, request, cancellationToken);
    }

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new SilentCookieLoginResponse(HttpStatusCode.OK)
    {
      Code = code
    };
  }

  private async Task<string> UpdateGrant(
    AuthorizationCodeGrant authorizationCodeGrant,
    SilentCookieLoginCommand request,
    CancellationToken cancellationToken)
  {
    var result = await _authorizationGrantService.UpdateAuthorizationGrant(
      new UpdateAuthorizationGrantArguments
      {
          AuthorizationCodeGrant = authorizationCodeGrant,
          Nonce = request.Nonce,
          Scope = request.Scope,
          CodeChallenge = request.CodeChallenge,
          CodeChallengeMethod = request.CodeChallengeMethod,
          RedirectUri = request.RedirectUri
      }, cancellationToken);

    return result.Code;
  }

  private async Task<string> CreateGrant(SilentCookieLoginCommand request, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(c => c.RedirectUris)
      .SingleAsync(c => c.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<User>()
      .Where(u => u.Id == request.UserId)
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
}
