namespace AuthServer.RequestAccessors.EndSession;

public class EndSessionRequest
{
    public required string IdTokenHint { get; init; }
    public required string ClientId { get; init; }
    public required string PostLogoutRedirectUri { get; init; }
    public required string State { get; init; }
}