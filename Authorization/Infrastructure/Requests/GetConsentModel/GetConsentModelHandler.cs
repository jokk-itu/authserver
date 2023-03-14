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

    var consentedClaims = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => x.Client.Id == request.ClientId)
      .SelectMany(x => x.ConsentedClaims)
      .Select(x => x.Name)
      .ToListAsync(cancellationToken: cancellationToken);

    var claims = ClaimsHelper
      .MapToClaims(request.Scope.Split(' '))
      .Select(x => new ClaimDto
      {
        Name = x,
        IsConsented = consentedClaims.Contains(x)
      });

    return new GetConsentModelResponse(HttpStatusCode.OK)
    {
      Claims = claims,
      ClientName = client.Name,
      GivenName = user.FirstName,
      PolicyUri = client.PolicyUri,
      TosUri = client.TosUri
    };
  }
}