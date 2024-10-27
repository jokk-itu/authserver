namespace AuthServer.TokenByGrant.RefreshTokenGrant;
internal class RefreshTokenValidatedRequest
{
    public required string ClientId { get; init; }
    public required string AuthorizationGrantId { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required IReadOnlyCollection<string> Resource { get; init; }
}