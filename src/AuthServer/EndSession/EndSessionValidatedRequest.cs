namespace AuthServer.EndSession;
internal class EndSessionValidatedRequest
{
    public required string? SubjectIdentifier { get; init; }
    public required string? SessionId { get; init; }
    public required string? ClientId { get; init; }
    public required bool LogoutAtIdentityProvider { get; init; }
}