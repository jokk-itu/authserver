using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;
public class AcrClaimMapper : IAcrClaimMapper
{
    public string MapAmrClaimToAcr(IReadOnlyCollection<string> amr)
    {
        if (amr.Contains(AmrValueConstants.MultiFactorAuthentication))
        {
            return "mfa";
        }
        else if (amr.Contains(AmrValueConstants.Sms))
        {
            return "2fa";
        }
        else if (amr.Contains(AmrValueConstants.Password))
        {
            return "pwd";
        }
        else
        {
            throw new InvalidOperationException("unknown amr");
        }
    }
}
