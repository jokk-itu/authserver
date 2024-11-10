using AuthServer.Authentication.Models;

namespace AuthServer.RequestAccessors.PushedAuthorization;
internal class PushedAuthorizationRequest
{
    public string? IdTokenHint { get; init; }
    public string? LoginHint { get; init; }
    public string? Prompt { get; init; }
    public string? Display { get; init; }
    public string? RedirectUri { get; init; }
    public string? CodeChallenge { get; init; }
    public string? CodeChallengeMethod { get; init; }
    public string? ResponseType { get; init; }
    public string? Nonce { get; init; }
    public string? MaxAge { get; init; }
    public string? State { get; init; }
    public string? ResponseMode { get; init; }
    public string? RequestObject { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; } = [];
    public required IReadOnlyCollection<string> AcrValues { get; init; } = [];
    public required IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; } = [];
}