namespace AuthServer.Introspection;
internal class IntrospectionValidatedRequest
{
    public required string ClientId { get; init; }
    public required string Token { get; init; }
}