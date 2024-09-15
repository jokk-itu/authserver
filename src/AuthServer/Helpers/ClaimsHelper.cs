using AuthServer.Constants;

namespace AuthServer.Helpers;
public static class ClaimsHelper
{
    /// <summary>
    /// Get ClaimNames from the given scope.
    /// </summary>
    /// <param name="scopes"></param>
    /// <returns></returns>
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
                    yield return ClaimNameConstants.Roles;
                    yield break;
                case ScopeConstants.Address:
                    yield return ClaimNameConstants.Address;
                    yield break;
                case ScopeConstants.Phone:
                    yield return ClaimNameConstants.PhoneNumber;
                    yield return ClaimNameConstants.PhoneNumberVerified;
                    yield break;
                case ScopeConstants.Email:
                    yield return ClaimNameConstants.Email;
                    yield return ClaimNameConstants.EmailVerified;
                    yield break;
            }
        }
    }
}
