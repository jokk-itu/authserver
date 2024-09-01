namespace AuthServer.Authorize.Abstractions;

public interface IAuthorizeUserAccessor
{
    /// <summary>
    /// Gets the <see cref="AuthorizeUser"/>.
    ///
    /// Throws an exception if <see cref="AuthorizeUser"/> is not set.
    /// </summary>
    /// <returns><see cref="AuthorizeUser"/></returns>
    AuthorizeUser GetUser();

    /// <summary>
    /// Gets the <see cref="AuthorizeUser"/> if it exists.
    /// </summary>
    /// <returns><see cref="AuthorizeUser"/> or null</returns>
    AuthorizeUser? TryGetUser();

    /// <summary>
    /// Sets an authenticated <see cref="AuthorizeUser"/>.
    ///
    /// Throws an exception if <see cref="AuthorizeUser"/> has already been set.
    /// </summary>
    /// <param name="authorizeUser"></param>
    void SetUser(AuthorizeUser authorizeUser);

    /// <summary>
    /// Tries to set <see cref="AuthorizeUser"/>.
    /// </summary>
    /// <param name="authorizeUser"></param>
    /// <returns>true if the <see cref="AuthorizeUser"/> is set, false otherwise.</returns>
    bool TrySetUser(AuthorizeUser authorizeUser);

    /// <summary>
    /// Clears the <see cref="AuthorizeUser"/> if it exists.
    /// </summary>
    /// <returns>true if a <see cref="AuthorizeUser"/> has been set and is then cleared, false if <see cref="AuthorizeUser"/> was not set and therefore cannot be cleared.</returns>
    bool ClearUser();
}