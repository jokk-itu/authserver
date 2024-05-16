namespace AuthServer.Introspection;
public interface IUsernameResolver
{
    /// <summary>
    /// Get a human-readable identifier for a user.
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <returns></returns>
    Task<string?> GetUsername(string subjectIdentifier);
}