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

    var userInfo = new List<KeyValuePair<string, object>> { new(ClaimNameConstants.Sub, subject) };

    foreach (var claim in query.ConsentedClaims.Select(x => x.Name))
    {
      switch (claim)
      {
        case ClaimNameConstants.Name:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.GetName()));
          break;
        case ClaimNameConstants.GivenName:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.FirstName));
          break;
        case ClaimNameConstants.FamilyName:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.LastName));
          break;
        case ClaimNameConstants.Address:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.Address));
          break;
        case ClaimNameConstants.Birthdate:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.Birthdate.ToString(CultureInfo.InvariantCulture)));
          break;
        case ClaimNameConstants.Locale:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.Locale));
          break;
        case ClaimNameConstants.Phone:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.PhoneNumber));
          break;
        case ClaimNameConstants.Email:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.Email));
          break;
        case ClaimNameConstants.Role:
          userInfo.Add(new KeyValuePair<string, object>(claim, query.User.Roles.Select(r => r.Value)));
          break;
      }
    }

    return userInfo;
  }
}