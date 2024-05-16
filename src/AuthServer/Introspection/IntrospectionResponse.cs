namespace AuthServer.Introspection;
internal class IntrospectionResponse
{
    public required bool Active { get; init; }
    public string? Scope { get; init; }
    public string? ClientId { get; init; }
    public string? Username { get; init; }
    public string? TokenType { get; init; }
    public long? ExpiresAt { get; init; }
    public long? IssuedAt { get; init; }
    public long? NotBefore { get; init; }
    public string? Subject { get; init; }
    public IEnumerable<string> Audience { get; init; } = [];
    public string? Issuer { get; init; }
    public string? JwtId { get; init; }
}