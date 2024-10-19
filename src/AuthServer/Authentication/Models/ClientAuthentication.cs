using AuthServer.Enums;

namespace AuthServer.Core.Models;
public abstract record ClientAuthentication(TokenEndpointAuthMethod Method);