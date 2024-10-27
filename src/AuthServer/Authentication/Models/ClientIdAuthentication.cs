using AuthServer.Enums;

namespace AuthServer.Core.Models;

internal record ClientIdAuthentication(string ClientId) : ClientAuthentication(TokenEndpointAuthMethod.None);