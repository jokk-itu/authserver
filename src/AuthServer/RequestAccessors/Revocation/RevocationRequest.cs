using AuthServer.Core.Models;

namespace AuthServer.RequestAccessors.Revocation;

public record RevocationRequest
{
    public required string Token { get; init; }
    public required string TokenTypeHint { get; init; }
    public required IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; }
}