using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;

public class AuthenticationContextReferenceResolver : IAuthenticationContextReferenceResolver
{
    public Task<string> ResolveAuthenticationContextReference(IReadOnlyCollection<string> authenticationMethodReferences,
        CancellationToken cancellationToken)
    {
        if (authenticationMethodReferences.Contains(AuthenticationMethodReferenceConstants.MultiFactorAuthentication))
        {
            return Task.FromResult(AuthenticationContextReferenceConstants.LevelOfAssuranceStrict);
        }
        else if (authenticationMethodReferences.Contains(AuthenticationMethodReferenceConstants.OneTimePassword))
        {
            return Task.FromResult(AuthenticationContextReferenceConstants.LevelOfAssuranceSubstantial);
        }
        else if (authenticationMethodReferences.Contains(AuthenticationMethodReferenceConstants.Password))
        {
            return Task.FromResult(AuthenticationContextReferenceConstants.LevelOfAssuranceLow);
        }

        throw new InvalidOperationException("unknown amr");
    }
}