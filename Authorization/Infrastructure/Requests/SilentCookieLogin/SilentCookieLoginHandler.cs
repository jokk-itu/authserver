using Domain;
using MediatR;
using System.Net;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentCookieLogin;
public class SilentCookieLoginHandler : AuthorizeHandler, IRequestHandler<SilentCookieLoginCommand, SilentCookieLoginResponse>
{
  private readonly IdentityContext _identityContext;
  
  public SilentCookieLoginHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService)
  : base(identityContext, authorizationGrantService)
  {
    _identityContext = identityContext;
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
      code = await CreateGrant(request, request.UserId, cancellationToken);
    }
    else
    {
      code = await UpdateGrant(request, authorizationGrant, cancellationToken);
    }

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new SilentCookieLoginResponse(HttpStatusCode.OK)
    {
      Code = code
    };
  }
}
