namespace AuthServer.Authorize.Abstractions;

public interface IAuthorizeUserAccessor
{
    /// <summary>
    /// Gets the authorizeUser.
    ///
    /// Throws an exception if authorizeUser is not set.
    /// </summary>
    /// <returns>authorizeUser</returns>
    AuthorizeUser GetUser();

    /// <summary>
    /// Gets the authorizeUser if it exists.
    /// </summary>
    /// <returns>authorizeUser or null</returns>
    AuthorizeUser? TryGetUser();

    /// <summary>
    /// Sets an authenticated authorizeUser.
    ///
    /// Throws an exception if an authorizeUser has already been set.
    /// </summary>
    /// <param name="authorizeUser"></param>
    void SetUser(AuthorizeUser authorizeUser);

    /// <summary>
    /// Tries to set an authorizeUser.
    /// </summary>
    /// <param name="authorizeUser"></param>
    /// <returns>true if the authorizeUser is set, false otherwise.</returns>
    bool TrySetUser(AuthorizeUser authorizeUser);

    /// <summary>
    /// Clears the authorizeUser if it exists.
    /// </summary>
    /// <returns>true if a authorizeUser has been set and is then cleared, false if a authorizeUser was not set and therefore cannot be cleared.</returns>
    bool ClearUser();
}