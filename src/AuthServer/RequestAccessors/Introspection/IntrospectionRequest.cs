using AuthServer.Authentication.Models;

namespace AuthServer.RequestAccessors.Introspection;

public class IntrospectionRequest
{
    public string? Token { get; init; }
    public string? TokenTypeHint { get; init; }
    public IReadOnlyCollection<ClientAuthentication> ClientAuthentications { get; init; } = [];
}
