using AuthServer.Enums;

namespace AuthServer.Core.Models;
public record ClientSecretAuthentication(TokenEndpointAuthMethod Method, string ClientId, string ClientSecret) : ClientAuthentication(Method);