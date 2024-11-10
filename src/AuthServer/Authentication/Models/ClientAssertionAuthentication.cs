using AuthServer.Enums;
using AuthServer.TokenDecoders;

namespace AuthServer.Authentication.Models;
internal record ClientAssertionAuthentication(ClientTokenAudience Audience, string? ClientId, string ClientAssertionType, string ClientAssertion) : ClientAuthentication(TokenEndpointAuthMethod.PrivateKeyJwt);