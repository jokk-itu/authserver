using AuthServer.Core.Models;

namespace AuthServer.RequestAccessors.Introspection;

public class IntrospectionRequest
{
    public required string Token { get; init; }
    public required string TokenTypeHint { get; init; }
    public required IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; }
}
