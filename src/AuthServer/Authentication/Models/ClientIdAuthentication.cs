using AuthServer.Enums;

namespace AuthServer.Authentication.Models;

internal record ClientIdAuthentication(string ClientId) : ClientAuthentication(TokenEndpointAuthMethod.None);