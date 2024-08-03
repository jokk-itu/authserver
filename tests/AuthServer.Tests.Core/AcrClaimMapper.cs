using System.Runtime.CompilerServices;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;
public class AcrClaimMapper : IAcrClaimMapper
{
    public string MapAmrClaimToAcr(string amr)
    {
        return amr switch
        {
            AmrValueConstants.Password => "pwd",
            AmrValueConstants.Sms => "2fa",
            AmrValueConstants.MultiFactorAuthentication => "mfa",
            _ => throw new SwitchExpressionException($"unknown amr claim value: {amr}")
        };
    }
}
