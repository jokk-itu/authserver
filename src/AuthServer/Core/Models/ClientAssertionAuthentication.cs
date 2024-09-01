using AuthServer.Enums;
using AuthServer.TokenDecoders;

namespace AuthServer.Core.Models;
internal record ClientAssertionAuthentication(ClientTokenAudience Audience, string? ClientId, string ClientAssertionType, string ClientAssertion) : ClientAuthentication(TokenEndpointAuthMethod.PrivateKeyJwt);