namespace AuthServer.TokenBuilders;
public class ClientAccessTokenArguments
{
    public required IReadOnlyCollection<string> Resource { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required string ClientId { get; init; }
}