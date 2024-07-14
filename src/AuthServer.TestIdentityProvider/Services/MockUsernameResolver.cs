using AuthServer.Introspection.Abstractions;

namespace AuthServer.TestIdentityProvider.Services;

public class MockUsernameResolver : IUsernameResolver
{
	public Task<string?> GetUsername(string subjectIdentifier)
    {
        return Task.FromResult((string?)"johndoe");
    }
}
