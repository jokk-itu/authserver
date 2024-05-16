using AuthServer.Constants;

namespace AuthServer.Helpers;

internal static class ClaimHelper
{
    /// <summary>
    /// Gets claim types based on scope.
    /// <remarks>https://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims</remarks>
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
                    yield return ClaimNameConstants.Birthdate;
                    yield return ClaimNameConstants.Locale;
                    break;
                case ScopeConstants.Phone:
                    yield return ClaimNameConstants.PhoneNumber;
                    yield return ClaimNameConstants.PhoneNumberVerified;
                    break;
                case ScopeConstants.Address:
                    yield return ClaimNameConstants.Address;
                    break;
                case ScopeConstants.Email:
                    yield return ClaimNameConstants.Email;
                    yield return ClaimNameConstants.EmailVerified;
                    break;
            }
        }
    }
}