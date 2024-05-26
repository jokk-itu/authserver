namespace AuthServer.Authorize;

internal class AuthorizeValidatedRequest
{
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }
    public required string ResponseType { get; init; }
    public required string Nonce { get; init; }
    public required string MaxAge { get; init; }
    public required string State { get; init; }
    public required string ResponseMode { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required IReadOnlyCollection<string> AcrValues { get; init; }
}