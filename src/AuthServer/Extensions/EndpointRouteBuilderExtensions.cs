using AuthServer.Constants;
using AuthServer.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AuthServer.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPostRegister(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapPost("connect/register", PostRegisterEndpoint.HandlePostRegister)
            .WithDisplayName("OpenId Connect Dynamic Client Registration")
            .WithName("OpenId Connect Dynamic Client Registration")
            .WithDescription("Endpoint to register a client")
            .WithGroupName("Register");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapUserinfo(this IEndpointRouteBuilder endpointBuilder)
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

    public static IEndpointRouteBuilder MapToken(this IEndpointRouteBuilder endpointBuilder)
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
            .MapMethods("oauth/revoke", ["POST"], RevocationEndpoint.HandleRevocation)
            .WithDisplayName("OAuth Revocation")
            .WithName("OAuth Revocation")
            .WithDescription("Endpoint to revoke a given token")
            .WithGroupName("Revocation");

        return endpointBuilder;
    }

    public static IEndpointRouteBuilder MapIntrospectionEndpoint(this IEndpointRouteBuilder endpointBuilder)
    {
        endpointBuilder
            .MapMethods("oauth/introspection", ["POST"], IntrospectionEndpoint.HandleIntrospection)
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