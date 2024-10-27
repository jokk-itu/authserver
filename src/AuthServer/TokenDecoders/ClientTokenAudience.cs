namespace AuthServer.TokenDecoders;
public enum ClientTokenAudience
{
    TokenEndpoint,
    AuthorizeEndpoint,
    IntrospectionEndpoint,
    RevocationEndpoint,
    PushedAuthorizeEndpoint
}