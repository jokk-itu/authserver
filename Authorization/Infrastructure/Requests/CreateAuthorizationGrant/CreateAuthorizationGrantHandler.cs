using System.Net;
using Domain;
using Domain.Enums;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantHandler : IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IAuthorizationGrantService _authorizationGrantService;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService)
  {
    _identityContext = identityContext;
    _authorizationGrantService = authorizationGrantService;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);

    var client = await _identityContext
      .Set<Client>()
      .Include(c => c.RedirectUris)
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<Session>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => !x.IsRevoked)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken) ?? new Session
      {
        User = user,
      };

    var currentAuthorizationGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(g => g.Session.Id == session.Id)
      .Where(g => g.Client.Id == request.ClientId)
      .Where(g => !g.IsRevoked)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (currentAuthorizationGrant is not null)
    {
      currentAuthorizationGrant.IsRevoked = true;
    }
    
    var maxAge = string.IsNullOrWhiteSpace(request.MaxAge) ? client.DefaultMaxAge : long.Parse(request.MaxAge);
    var redirectUri = string.IsNullOrWhiteSpace(request.RedirectUri)
      ? client.RedirectUris.Single(r => r.Type == RedirectUriType.AuthorizeRedirectUri).Uri
      : request.RedirectUri;

    var result = await _authorizationGrantService.CreateAuthorizationGrant(
      new CreateAuthorizationGrantArguments
      {
        Session = session,
        Client = client,
        CodeChallenge = request.CodeChallenge,
        CodeChallengeMethod = request.CodeChallengeMethod,
        RedirectUri = redirectUri,
        MaxAge = maxAge,
        Nonce = request.Nonce,
        Scope = request.Scope
      }, cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateAuthorizationGrantResponse(HttpStatusCode.OK)
    {
      Code = result.Code,
      State = request.State
    };
  }
}