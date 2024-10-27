namespace AuthServer.PushedAuthorization;
internal class PushedAuthorizationResponse
{
    public required string RequestUri { get; init; }
    public required int ExpiresIn { get; init; }
    public required string ClientId { get; init; }
}
