using Domain.Constants;
using Domain;
using Infrastructure.Services.Abstract;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;
public class ClaimService : IClaimService
{
  private readonly IdentityContext _identityContext;

  public ClaimService(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<IDictionary<string, string>> GetClaimsFromConsentGrant(string userId, string clientId, CancellationToken cancellationToken = default)
  {
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Where(cg => cg.User.Id == userId)
      .Where(cg => cg.Client.Id == clientId)
      .Include(cg => cg.ConsentedClaims)
      .Include(cg => cg.User)
      .Include(cg => cg.Client)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var userInfo = new Dictionary<string, string> { { ClaimNameConstants.Sub, userId } };

    if (consentGrant is null)
    {
      return userInfo;
    }

    foreach (var claim in consentGrant.ConsentedClaims.Select(x => x.Name))
    {
      switch (claim)
      {
        case ClaimNameConstants.Name:
          userInfo.Add(claim, consentGrant.User.GetName());
          break;
        case ClaimNameConstants.GivenName:
          userInfo.Add(claim, consentGrant.User.FirstName);
          break;
        case ClaimNameConstants.FamilyName:
          userInfo.Add(claim, consentGrant.User.LastName);
          break;
        case ClaimNameConstants.Address:
          userInfo.Add(claim, consentGrant.User.Address);
          break;
        case ClaimNameConstants.Birthdate:
          userInfo.Add(claim, consentGrant.User.Birthdate.ToString(CultureInfo.InvariantCulture));
          break;
        case ClaimNameConstants.Locale:
          userInfo.Add(claim, consentGrant.User.Locale);
          break;
        case ClaimNameConstants.Phone:
          userInfo.Add(claim, consentGrant.User.PhoneNumber);
          break;
        case ClaimNameConstants.Email:
          userInfo.Add(claim, consentGrant.User.Email);
          break;
      }
    }

    return userInfo;
  }
}