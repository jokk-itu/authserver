using System.Security.Claims;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;

namespace AuthServer.TestIdentityProvider.Services;

public class MockUserClaimService : IUserClaimService
{
	public Task<IEnumerable<Claim>> GetClaims(string subjectIdentifier, CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<Claim>)
        [
            new Claim(ClaimNameConstants.GivenName, "John"),
            new Claim(ClaimNameConstants.FamilyName, "Doe")
        ]);
    }
}
