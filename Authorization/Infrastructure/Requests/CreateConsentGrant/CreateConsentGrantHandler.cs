using System.Net;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateConsentGrant;
public class CreateConsentGrantHandler : IRequestHandler<CreateConsentGrantCommand, CreateConsentGrantResponse>
{
  private readonly IdentityContext _identityContext;

  public CreateConsentGrantHandler(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<CreateConsentGrantResponse> Handle(CreateConsentGrantCommand request, CancellationToken cancellationToken)
  {
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedClaims)
      .Include(x => x.ConsentedScopes)
      .Include(x => x.Client)
      .Include(x => x.User)
      .Where(x => x.Client.Id == request.ClientId)
      .Where(x => x.User.Id == request.UserId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var claims = await _identityContext
      .Set<Claim>()
      .Where(x => request.ConsentedClaims.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.ConsentedScopes.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    if (consentGrant is not null)
    {
      consentGrant.Updated = DateTime.UtcNow;
      consentGrant.ConsentedClaims = claims;
      consentGrant.ConsentedScopes = scopes;
    }
    else
    {
      var client = await _identityContext
        .Set<Client>()
        .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

      var user = await _identityContext
        .Set<User>()
        .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);

      if (client.SubjectType == SubjectType.Pairwise)
      {
        var pairwiseIdentifier = new PairwiseIdentifier
        {
          Client = client,
          User = user
        };
        await _identityContext
          .Set<PairwiseIdentifier>()
          .AddAsync(pairwiseIdentifier, cancellationToken: cancellationToken);
      }

      consentGrant = new ConsentGrant
      {
        Client = client,
        User = user,
        ConsentedClaims = claims,
        ConsentedScopes = scopes,
        Updated = DateTime.UtcNow
      };

      await _identityContext
        .Set<ConsentGrant>()
        .AddAsync(consentGrant, cancellationToken);
    }
    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateConsentGrantResponse(HttpStatusCode.OK);
  }
}
