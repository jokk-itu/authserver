using Domain.Constants;
using Domain;
using System.Globalization;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Services;
public class ClaimService : IClaimService
{
  private readonly IdentityContext _identityContext;

  public ClaimService(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<IEnumerable<KeyValuePair<string, object>>> GetClaimsFromConsentGrant(string userId, string clientId, CancellationToken cancellationToken = default)
  {
    var query = await _identityContext
      .Set<ConsentGrant>()
      .Where(cg => cg.User.Id == userId)
      .Where(cg => cg.Client.Id == clientId)
      .Include(cg => cg.ConsentedClaims)
      .Include(cg => cg.User)
      .ThenInclude(u => u.Roles)
      .Include(cg => cg.Client)
      .Select(cg => new
      {
        cg.Client,
        cg.User,
        cg.ConsentedClaims,
        Subject = cg.User.PairwiseIdentifiers.SingleOrDefault(pi => pi.Client.Id == clientId)
      })
      .SingleAsync(cancellationToken: cancellationToken);

    var subject = query.Client.SubjectType == SubjectType.Public
      ? userId
      : query.Subject!.Id;

    var userInfo = new Dictionary<string, object>
    {
      { ClaimNameConstants.Sub, subject }
    };

    foreach (var claim in query.ConsentedClaims.Select(x => x.Name))
    {
      switch (claim)
      {
        case ClaimNameConstants.Name when !string.IsNullOrWhiteSpace(query.User.GetName()):
          userInfo.Add(claim, query.User.GetName());
          break;
        case ClaimNameConstants.GivenName when !string.IsNullOrWhiteSpace(query.User.FirstName):
          userInfo.Add(claim, query.User.FirstName);
          break;
        case ClaimNameConstants.FamilyName when !string.IsNullOrWhiteSpace(query.User.LastName):
          userInfo.Add(claim, query.User.LastName);
          break;
        case ClaimNameConstants.Address when !string.IsNullOrWhiteSpace(query.User.Address):
          userInfo.Add(claim, query.User.Address);
          break;
        case ClaimNameConstants.Birthdate:
          userInfo.Add(claim, query.User.Birthdate.ToString(CultureInfo.InvariantCulture));
          break;
        case ClaimNameConstants.Locale when !string.IsNullOrWhiteSpace(query.User.Locale):
          userInfo.Add(claim, query.User.Locale);
          break;
        case ClaimNameConstants.Phone when !string.IsNullOrWhiteSpace(query.User.PhoneNumber):
          userInfo.Add(claim, query.User.PhoneNumber);
          break;
        case ClaimNameConstants.Email when !string.IsNullOrWhiteSpace(query.User.Email):
          userInfo.Add(claim, query.User.Email);
          break;
        case ClaimNameConstants.Role when query.User.Roles.Any():
          userInfo.Add(claim, query.User.Roles.Select(r => r.Value));
          break;
      }
    }

    return userInfo;
  }
}