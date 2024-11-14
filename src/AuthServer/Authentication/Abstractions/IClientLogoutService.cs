namespace AuthServer.Authentication.Abstractions;

internal interface IClientLogoutService
{
    /// <summary>
    /// Requests the client's backchannel logout endpoint.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="sessionId"></param>
    /// <param name="subjectIdentifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Logout(string clientId, string? sessionId, string? subjectIdentifier, CancellationToken cancellationToken);
}