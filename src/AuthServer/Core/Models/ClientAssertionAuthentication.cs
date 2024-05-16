using AuthServer.Enums;
using AuthServer.TokenDecoders;

namespace AuthServer.Core.Models;
public record ClientAssertionAuthentication(ClientTokenAudience Audience, string? ClientId, string ClientAssertionType, string ClientAssertion) : ClientAuthentication(TokenEndpointAuthMethod.PrivateKeyJwt);