namespace AuthServer.TokenBuilders;
public class LogoutTokenArguments
{
    public required string ClientId { get; init; }
    public required string? SubjectIdentifier { get; init; }
    public required string? SessionId { get; init; }
}