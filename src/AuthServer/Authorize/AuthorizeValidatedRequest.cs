namespace AuthServer.Authorize;

internal class AuthorizeValidatedRequest
{
    public required string SubjectIdentifier { get; init; }
    public required string ClientId { get; init; }
    public string? RedirectUri { get; init; }
    public required string CodeChallenge { get; init; }
    public required string Nonce { get; init; }
    public string? ResponseMode { get; init; }
    public string? RequestUri { get; init; }
    public IReadOnlyCollection<string> Scope { get; init; } = [];
    public IReadOnlyCollection<string> AcrValues { get; init; } = [];
}