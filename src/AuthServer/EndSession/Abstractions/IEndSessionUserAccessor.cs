namespace AuthServer.EndSession.Abstractions;
public interface IEndSessionUserAccessor
{
    /// <summary>
    /// Gets the endSessionUser.
    ///
    /// Throws an exception if endSessionUser is not set.
    /// </summary>
    /// <returns>endSessionUser</returns>
    EndSessionUser GetUser();

    /// <summary>
    /// Gets the endSessionUser if it exists.
    /// </summary>
    /// <returns>endSessionUser or null</returns>
    EndSessionUser? TryGetUser();

    /// <summary>
    /// Sets an authenticated endSessionUser.
    ///
    /// Throws an exception if an endSessionUser has already been set.
    /// </summary>
    /// <param name="endSessionUser"></param>
    void SetUser(EndSessionUser endSessionUser);

    /// <summary>
    /// Tries to set an endSessionUser.
    /// </summary>
    /// <param name="endSessionUser"></param>
    /// <returns>true if the endSessionUser is set, false otherwise.</returns>
    bool TrySetUser(EndSessionUser endSessionUser);

    /// <summary>
    /// Clears the endSessionUser if it exists.
    /// </summary>
    /// <returns>true if a endSessionUser has been set and is then cleared, false if a endSessionUser was not set and therefore cannot be cleared.</returns>
    bool ClearUser();
}