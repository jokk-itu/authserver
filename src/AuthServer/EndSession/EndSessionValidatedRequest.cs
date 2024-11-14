namespace AuthServer.EndSession;
internal class EndSessionValidatedRequest
{
    public string? SubjectIdentifier { get; init; }
    public string? SessionId { get; init; }
    public string? ClientId { get; init; }
    public bool LogoutAtIdentityProvider { get; init; }
}