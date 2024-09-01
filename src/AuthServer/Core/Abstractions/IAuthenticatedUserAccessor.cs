namespace AuthServer.Core.Abstractions;

public interface IAuthenticatedUserAccessor
{
    Task<AuthenticatedUser?> GetAuthenticatedUser();
    Task<int> CountAuthenticatedUsers();
}