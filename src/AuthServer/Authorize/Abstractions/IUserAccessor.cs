namespace AuthServer.Authorize.Abstractions;

public interface IUserAccessor
{
    /// <summary>
    /// Gets the authenticatedUser.
    ///
    /// Throws an exception if authenticatedUser is not set.
    /// </summary>
    /// <returns>authenticatedUser</returns>
    AuthenticatedUser GetUser();

    /// <summary>
    /// Gets the authenticatedUser if it exists.
    /// </summary>
    /// <returns>authenticatedUser or null</returns>
    AuthenticatedUser? TryGetUser();

    /// <summary>
    /// Sets an authenticated authenticatedUser.
    ///
    /// Throws an exception if an authenticatedUser has already been set.
    /// </summary>
    /// <param name="authenticatedUser"></param>
    void SetUser(AuthenticatedUser authenticatedUser);

    /// <summary>
    /// Tries to set an authenticatedUser.
    /// </summary>
    /// <param name="authenticatedUser"></param>
    /// <returns>true if the authenticatedUser is set, false otherwise.</returns>
    bool TrySetUser(AuthenticatedUser authenticatedUser);

    /// <summary>
    /// Clears the authenticatedUser if it exists.
    /// </summary>
    /// <returns>true if a authenticatedUser has been set and is then cleared, false if a authenticatedUser was not set and therefore cannot be cleared.</returns>
    bool ClearUser();
}