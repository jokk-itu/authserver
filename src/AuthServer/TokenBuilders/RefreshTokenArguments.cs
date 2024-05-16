namespace AuthServer.TokenBuilders;
public class RefreshTokenArguments
{
    public required string AuthorizationGrantId { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
}