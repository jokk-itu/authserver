using System.Net;
using Domain;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetConsentModel;
public class GetConsentModelHandler : IRequestHandler<GetConsentModelQuery, GetConsentModelResponse>
{
  private readonly IdentityContext _identityContext;

  public GetConsentModelHandler(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<GetConsentModelResponse> Handle(GetConsentModelQuery request, CancellationToken cancellationToken)
  {
    var user = await _identityContext
      .Set<User>()
      .Where(x => x.Id == request.UserId)
      .SingleAsync(cancellationToken: cancellationToken);

    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == request.ClientId)
      .SingleAsync(cancellationToken: cancellationToken);

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => x.Client.Id == request.ClientId)
      .Include(x => x.ConsentedClaims)
      .Include(x => x.ConsentedScopes)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var claims = ClaimsHelper
      .MapToClaims(request.Scope.Split(' '))
      .Select(x => new ClaimDto
      {
        Name = x,
        IsConsented = consentGrant?.ConsentedClaims.Any(y => y.Name == x) == true
      });

    var scopes = consentGrant?.ConsentedScopes
      .Select(x => new ScopeDto
      {
        Name = x.Name
      })
      .ToList() ?? new List<ScopeDto>();

    return new GetConsentModelResponse(HttpStatusCode.OK)
    {
      Claims = claims,
      Scopes = scopes,
      ClientName = client.Name,
      GivenName = user.FirstName,
      PolicyUri = client.PolicyUri,
      TosUri = client.TosUri,
      LogoUri = client.LogoUri
    };
  }
}