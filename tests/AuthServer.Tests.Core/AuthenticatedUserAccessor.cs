using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;

namespace AuthServer.Tests.Core;
public class AuthenticatedUserAccessor : IAuthenticatedUserAccessor
{
    public Task<AuthenticatedUser?> GetAuthenticatedUser()
    {
        return Task.FromResult<AuthenticatedUser?>(
            new AuthenticatedUser(UserConstants.SubjectIdentifier));
    }

    public Task<int> CountAuthenticatedUsers()
    {
        return Task.FromResult(0);
    }
}
