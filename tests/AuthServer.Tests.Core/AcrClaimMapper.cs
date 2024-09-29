using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;
public class AcrClaimMapper : IAcrClaimMapper
{
    public string MapAmrClaimToAcr(IReadOnlyCollection<string> amr)
    {
        if (amr.Contains(AuthenticationMethodReferenceConstants.MultiFactorAuthentication))
        {
            return "mfa";
        }
        else if (amr.Contains(AuthenticationMethodReferenceConstants.Sms))
        {
            return "2fa";
        }
        else if (amr.Contains(AuthenticationMethodReferenceConstants.Password))
        {
            return "pwd";
        }
        else
        {
            throw new InvalidOperationException("unknown amr");
        }
    }
}
