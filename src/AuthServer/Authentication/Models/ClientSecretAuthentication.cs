using AuthServer.Enums;

namespace AuthServer.Authentication.Models;
public record ClientSecretAuthentication(TokenEndpointAuthMethod Method, string ClientId, string ClientSecret) : ClientAuthentication(Method);