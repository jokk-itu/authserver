namespace AuthServer.Userinfo;
internal class UserinfoValidatedRequest
{
    public required string AuthorizationGrantId { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
}