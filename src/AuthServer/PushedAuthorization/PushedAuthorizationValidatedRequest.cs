namespace AuthServer.PushedAuthorization;
internal class PushedAuthorizationValidatedRequest
{
    public string? IdTokenHint { get; init; }
    public string? LoginHint { get; init; }
    public string? Prompt { get; init; }
    public string? Display { get; init; }
    public required string ClientId { get; init; }
    public string? RedirectUri { get; init; }
    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }
    public required string ResponseType { get; init; }
    public required string Nonce { get; init; }
    public string? MaxAge { get; init; }
    public required string State { get; init; }
    public string? ResponseMode { get; init; }
    public IReadOnlyCollection<string> Scope { get; init; } = [];
    public IReadOnlyCollection<string> AcrValues { get; init; } = [];
}
