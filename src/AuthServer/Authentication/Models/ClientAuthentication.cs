using AuthServer.Enums;

namespace AuthServer.Authentication.Models;
public abstract record ClientAuthentication(TokenEndpointAuthMethod Method);