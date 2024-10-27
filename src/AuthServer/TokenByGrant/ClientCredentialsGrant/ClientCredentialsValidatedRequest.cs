namespace AuthServer.TokenByGrant.ClientCredentialsGrant;
internal class ClientCredentialsValidatedRequest
{
    public required string ClientId { get; init; }
    public required IReadOnlyCollection<string> Resource { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
}