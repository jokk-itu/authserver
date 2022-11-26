using Domain.Constants;

namespace Infrastructure.Helpers;
public static class ClaimsHelper
{
  public static IEnumerable<string> MapToClaims(IEnumerable<string> scopes)
  {
    foreach (var scope in scopes.Distinct())
    {
      switch (scope)
      {
        case ScopeConstants.Profile:
          yield return ClaimNameConstants.Name;
          yield return ClaimNameConstants.FamilyName;
          yield return ClaimNameConstants.GivenName;
          yield return ClaimNameConstants.Address;
          yield return ClaimNameConstants.Birthdate;
          yield return ClaimNameConstants.Locale;
          break;
        case ScopeConstants.Phone:
          yield return ClaimNameConstants.Phone;
          break;
        case ScopeConstants.Email:
          yield return ClaimNameConstants.Email;
          break;
      }
    }
  }
}
