using AuthServer.Authentication.Models;

namespace AuthServer.RequestAccessors.Token;

public class TokenRequest
{
    public string? GrantType { get; init; }
    public string? Code { get; init; }
    public string? CodeVerifier { get; init; }
    public string? RedirectUri { get; init; }
    public string? RefreshToken { get; init; }
    public string? DPoPToken { get; init; }
    public IReadOnlyCollection<string> Scope { get; init; } = [];
    public IReadOnlyCollection<string> Resource { get; init; } = [];
    public IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; } = [];
}