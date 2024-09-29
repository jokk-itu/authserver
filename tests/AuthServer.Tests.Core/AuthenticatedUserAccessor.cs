using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;

namespace AuthServer.Tests.Core;
public class AuthenticatedUserAccessor : IAuthenticatedUserAccessor
{
    public Task<AuthenticatedUser?> GetAuthenticatedUser()
    {
        return Task.FromResult<AuthenticatedUser?>(
            new AuthenticatedUser(UserConstants.SubjectIdentifier, [AuthenticationMethodReferenceConstants.Password]));
    }

    public Task<int> CountAuthenticatedUsers()
    {
        return Task.FromResult(0);
    }
}
