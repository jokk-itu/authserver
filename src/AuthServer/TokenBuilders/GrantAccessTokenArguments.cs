namespace AuthServer.TokenBuilders;
public class GrantAccessTokenArguments
{
    public required IReadOnlyCollection<string> Resource { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required string AuthorizationGrantId { get; init; }
}