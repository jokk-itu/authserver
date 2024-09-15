using AuthServer.Constants;
using AuthServer.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthServer.Extensions;
public static class ApplicationBuilderExtensions
{
    public static IEndpointRouteBuilder MapPushedAuthorizationEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/par", ["POST"], PushedAuthorizationRequestEndpoint.HandlePushedAuthorization)
            .WithDisplayName("OpenId Connect Pushed Authorization")
            .WithName("OpenId Connect Pushed Authorization")
            .WithDescription("OpenId Connect Pushed Authorization")
            .WithGroupName("Authorize");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapEndSessionEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/end-session", ["GET", "POST"], EndSessionEndpoint.HandleEndSession)
            .WithDisplayName("OpenId Connect EndSession")
            .WithName("OpenId Connect EndSession")
            .WithDescription("Endpoint to end the session")
            .WithGroupName("EndSession");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapDynamicClientRegistrationEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/register", ["POST"], RegisterEndpoint.HandleRegister)
            .WithDisplayName("OpenId Connect Dynamic Registration")
            .WithName("OpenId Connect Dynamic Registration")
            .WithDescription("Endpoint to register a client")
            .WithGroupName("Register");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapDynamicClientManagementEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/register", ["GET", "PUT", "DELETE"], RegisterEndpoint.HandleRegister)
            .WithDisplayName("OpenId Connect Dynamic Management")
            .WithName("OpenId Connect Dynamic Management")
            .WithDescription("Endpoint to manage a client")
            .WithGroupName("Register")
            .RequireAuthorization(AuthorizationConstants.Register);

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapUserinfoEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/userinfo", ["GET", "POST"], UserinfoEndpoint.HandleUserinfo)
            .WithDisplayName("OpenId Connect Userinfo")
            .WithName("OpenId Connect Userinfo")
            .WithDescription("Endpoint to get userinfo")
            .WithGroupName("Userinfo")
            .RequireAuthorization(AuthorizationConstants.Userinfo);

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapAuthorizeEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/authorize", ["GET", "POST"], AuthorizeEndpoint.HandleAuthorize)
            .WithDisplayName("OpenId Connect Authorize")
            .WithName("OpenId Connect Authorize")
            .WithDescription("OpenId Connect Authorize")
            .WithGroupName("Authorize");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapTokenEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/token", ["POST"], TokenEndpoint.HandleToken)
            .WithDisplayName("OpenId Connect Token")
            .WithName("OpenId Connect Token")
            .WithDescription("Endpoint to get tokens")
            .WithGroupName("Token");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapRevocationEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/revoke", ["POST"], RevocationEndpoint.HandleRevocation)
            .WithDisplayName("OAuth Revocation")
            .WithName("OAuth Revocation")
            .WithDescription("Endpoint to revoke a given token")
            .WithGroupName("Revocation");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapIntrospectionEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("connect/introspection", ["POST"], IntrospectionEndpoint.HandleIntrospection)
            .WithDisplayName("OAuth Introspection")
            .WithName("OAuth Introspection")
            .WithDescription("Endpoint to get a structured token from a reference")
            .WithGroupName("Introspection");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapDiscoveryDocumentEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods(".well-known/openid-configuration", ["GET"], DiscoveryDocumentEndpoint.HandleDiscoveryDocument)
            .WithDisplayName("OpenIdConnect Configuration")
            .WithName("OpenIdConnect Configuration")
            .WithDescription("Endpoint to get the configuration")
            .WithGroupName("Configuration");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapJwksDocumentEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods(".well-known/jwks", ["GET"], JwksDocumentEndpoint.HandleJwksDocument)
            .WithDisplayName("OAuth JWKS")
            .WithName("OAuth JWKS")
            .WithDescription("Endpoint to get the jwks")
            .WithGroupName("Configuration");

        return endpointBuilder;
    }
}
