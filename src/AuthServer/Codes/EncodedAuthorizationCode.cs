namespace AuthServer.Codes;
internal class EncodedAuthorizationCode
{
    public required string AuthorizationGrantId { get; init; }
    public required string AuthorizationCodeId { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required string CodeChallenge { get; init; }
    public string? RedirectUri { get; init; }
}