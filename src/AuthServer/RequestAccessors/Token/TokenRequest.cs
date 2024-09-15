using AuthServer.Core.Models;

namespace AuthServer.RequestAccessors.Token;

public class TokenRequest
{
    public required string GrantType { get; init; }
    public required string Code { get; init; }
    public required string CodeVerifier { get; init; }
    public required string RedirectUri { get; init; }
    public required string RefreshToken { get; init; }
    public required string DPoPToken { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required IReadOnlyCollection<string> Resource { get; init; }
    public required IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; }
}