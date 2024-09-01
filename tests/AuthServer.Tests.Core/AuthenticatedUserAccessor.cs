using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;

namespace AuthServer.Tests.Core;
internal class AuthenticatedUserAccessor : IAuthenticatedUserAccessor
{
    public Task<AuthenticatedUser?> GetAuthenticatedUser()
    {
        return Task.FromResult<AuthenticatedUser?>(
            new AuthenticatedUser(UserConstants.SubjectIdentifier, [AmrValueConstants.Password]));
    }

    public Task<int> CountAuthenticatedUsers()
    {
        return Task.FromResult(1);
    }
}
