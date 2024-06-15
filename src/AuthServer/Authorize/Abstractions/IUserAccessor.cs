namespace AuthServer.Authorize.Abstract;

internal interface IUserAccessor
{
    /// <summary>
    /// Gets the authenticated user.
    ///
    /// Throws an exception if it is not set.
    /// </summary>
    /// <returns>authenticated user</returns>
    User GetUser();

    /// <summary>
    /// Gets the authenticated user if it exists.
    /// </summary>
    /// <returns>authenticated user or null</returns>
    User? TryGetUser();

    /// <summary>
    /// Sets an authenticated user.
    ///
    /// Throws an exception if a user has already been set.
    /// </summary>
    /// <param name="user"></param>
    void SetUser(User user);

    /// <summary>
    /// Tries to set an authenticated user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>true if the user is set, false otherwise.</returns>
    bool TrySetUser(User user);
}

public record User(string SubjectIdentifier, IEnumerable<string> Amr);