using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.OAuthToken;
using AuthServer.Authorization;
using AuthServer.Authorization.Abstractions;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Cache;
using AuthServer.Cache.Abstractions;
using AuthServer.Codes;
using AuthServer.Codes.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.EndSession;
using AuthServer.EndSession.Abstractions;
using AuthServer.Introspection;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.PushedAuthorization;
using AuthServer.Register;
using AuthServer.Repositories;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.RequestAccessors.EndSession;
using AuthServer.RequestAccessors.Introspection;
using AuthServer.RequestAccessors.PushedAuthorization;
using AuthServer.RequestAccessors.Register;
using AuthServer.RequestAccessors.Revocation;
using AuthServer.RequestAccessors.Token;
using AuthServer.RequestAccessors.Userinfo;
using AuthServer.Revocation;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.AuthorizationCodeGrant;
using AuthServer.TokenByGrant.ClientCredentialsGrant;
using AuthServer.TokenByGrant.RefreshTokenGrant;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using AuthServer.Userinfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServer(this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> databaseConfigurator)
    {
        services
            .AddAuthentication()
            .AddScheme<OAuthTokenAuthenticationOptions, OAuthTokenAuthenticationHandler>(
                OAuthTokenAuthenticationDefaults.AuthenticationScheme, null);

        services
            .AddAuthorizationBuilder()
            .AddPolicy(AuthorizationConstants.Userinfo, policy =>
            {
                policy.AddAuthenticationSchemes(OAuthTokenAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAssertion(context =>
                {
                    var scope = context.User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Scope)?.Value;
                    return scope is not null && scope.Split(' ').Contains(ScopeConstants.UserInfo);
                });
            })
            .AddPolicy(AuthorizationConstants.Register, policy =>
            {
                policy.AddAuthenticationSchemes(OAuthTokenAuthenticationDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.Register);
            });

        services.AddDataProtection();
        services.AddSingleton<IMetricService, MetricService>();
        services.AddHttpContextAccessor();
        services.AddHttpClient(HttpClientNameConstants.Client);

        services
            .ConfigureOptions<PostConfigureDiscoveryDocumentOptions>()
            .ConfigureOptions<ValidateDiscoveryDocumentOptions>()
            .ConfigureOptions<ValidateJwksDocument>()
            .ConfigureOptions<ValidateUserInteractionOptions>();

        services
            .AddDbContext<AuthorizationDbContext>(databaseConfigurator)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<ICachedClientStore, CachedClientStore>()
            .AddScoped<ITokenReplayCache, TokenReplayCache>();

        services
            .AddScoped<ITokenBuilder<LogoutTokenArguments>, LogoutTokenBuilder>()
            .AddScoped<ITokenBuilder<IdTokenArguments>, IdTokenBuilder>()
            .AddScoped<ITokenBuilder<ClientAccessTokenArguments>, ClientAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<GrantAccessTokenArguments>, GrantAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<RefreshTokenArguments>, RefreshTokenBuilder>()
            .AddScoped<ITokenBuilder<RegistrationTokenArguments>, RegistrationTokenBuilder>()
            .AddScoped<ITokenBuilder<UserinfoTokenArguments>, UserinfoTokenBuilder>()
            .AddScoped<ITokenSecurityService, TokenSecurityService>();

        services
            .AddScoped<ITokenDecoder<ServerIssuedTokenDecodeArguments>, ServerIssuedTokenDecoder>()
            .AddScoped<ITokenDecoder<ClientIssuedTokenDecodeArguments>, ClientIssuedTokenDecoder>()
            .AddScoped<IAuthorizationCodeEncoder, AuthorizationCodeEncoder>();

        services
            .AddScoped<IClientAuthenticationService, ClientAuthenticationService>()
            .AddScoped<IClientJwkService, ClientJwkService>()
            .AddScoped<IClientSectorService, ClientSectorService>()
            .AddScoped<IClientLogoutService, ClientLogoutService>();

        services
            .AddScoped<IClientRepository, ClientRepository>()
            .AddScoped<IConsentGrantRepository, ConsentGrantRepository>()
            .AddScoped<IAuthorizationGrantRepository, AuthorizationGrantRepository>()
            .AddScoped<ITokenRepository, TokenRepository>()
            .AddScoped<INonceRepository, NonceRepository>()
            .AddScoped<ISessionRepository, SessionRepository>();

        AddAuthorize(services);
        AddToken(services);
        AddUserinfo(services);
        AddEndSession(services);
        AddIntrospection(services);
        AddRevocation(services);
        AddPushedAuthorization(services);
        AddRegister(services);

        return services;
    }

    internal static IServiceCollection AddPushedAuthorization(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IEndpointHandler, PushedAuthorizationEndpointHandler>("PushedAuthorization")
            .AddSingleton<IEndpointModule, PushedAuthorizationEndpointModule>()
            .AddScoped<IRequestAccessor<PushedAuthorizationRequest>, PushedAuthorizationRequestAccessor>()
            .AddScoped<IRequestHandler<PushedAuthorizationRequest, PushedAuthorizationResponse>, PushedAuthorizationRequestHandler>()
            .AddScoped<IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>, PushedAuthorizationRequestProcessor>()
            .AddScoped<IRequestValidator<PushedAuthorizationRequest, PushedAuthorizationValidatedRequest>, PushedAuthorizationRequestValidator>();
    }

    internal static IServiceCollection AddRegister(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<RegisterRequest>, RegisterRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, RegisterEndpointHandler>("Register")
            .AddSingleton<IEndpointModule, RegisterEndpointModule>()
            .AddScoped<IRequestHandler<RegisterRequest, ProcessResult<RegisterResponse, Unit>>, RegisterRequestHandler>()
            .AddScoped<IRequestValidator<RegisterRequest, RegisterValidatedRequest>, RegisterRequestValidator>()
            .AddScoped<IRequestProcessor<RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>>, RegisterRequestProcessor>();
    }

    internal static IServiceCollection AddEndSession(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<EndSessionRequest>, EndSessionRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, EndSessionEndpointHandler>("EndSession")
            .AddSingleton<IEndpointModule, EndSessionEndpointModule>()
            .AddScoped<IEndSessionUserAccessor, EndSessionUserAccessor>()
            .AddScoped<IRequestHandler<EndSessionRequest, Unit>, EndSessionRequestHandler>()
            .AddScoped<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>, EndSessionRequestValidator>()
            .AddScoped<IRequestProcessor<EndSessionValidatedRequest, Unit>, EndSessionRequestProcessor>();
    }

    internal static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<AuthorizeRequest>, AuthorizeRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, AuthorizeEndpointHandler>("Authorize")
            .AddSingleton<IEndpointModule, AuthorizeEndpointModule>()
            .AddScoped<IAuthorizeService, AuthorizeService>()
            .AddScoped<IAuthorizeInteractionService, AuthorizeInteractionService>()
            .AddScoped<IAuthorizeResponseBuilder, AuthorizeResponseBuilder>()
            .AddScoped<IAuthorizeUserAccessor, AuthorizeUserAccessor>()
            .AddScoped<ISecureRequestService, SecureRequestService>()
            .AddScoped<IRequestHandler<AuthorizeRequest, string>, AuthorizeRequestHandler>()
            .AddScoped<IRequestProcessor<AuthorizeValidatedRequest, string>, AuthorizeRequestProcessor>()
            .AddScoped<IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>, AuthorizeRequestValidator>();
    }

    internal static IServiceCollection AddUserinfo(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<UserinfoRequest>, UserinfoRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, UserinfoEndpointHandler>("Userinfo")
            .AddSingleton<IEndpointModule, UserinfoEndpointModule>()
            .AddScoped<IRequestHandler<UserinfoRequest, string>, UserinfoRequestHandler>()
            .AddScoped<IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>, UserinfoRequestValidator>()
            .AddScoped<IRequestProcessor<UserinfoValidatedRequest, string>, UserinfoRequestProcessor>();
    }

    internal static IServiceCollection AddIntrospection(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<IntrospectionRequest>, IntrospectionRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, IntrospectionEndpointHandler>("Introspection")
            .AddSingleton<IEndpointModule, IntrospectionEndpointModule>()
            .AddScoped<IRequestHandler<IntrospectionRequest, IntrospectionResponse>, IntrospectionRequestHandler>()
            .AddScoped<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>, IntrospectionRequestValidator>()
            .AddScoped<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>, IntrospectionRequestProcessor>();
    }

    internal static IServiceCollection AddRevocation(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<RevocationRequest>, RevocationRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, RevocationEndpointHandler>("Revocation")
            .AddSingleton<IEndpointModule, RevocationEndpointModule>()
            .AddScoped<IRequestHandler<RevocationRequest, Unit>, RevocationRequestHandler>()
            .AddScoped<IRequestValidator<RevocationRequest, RevocationValidatedRequest>, RevocationRequestValidator>()
            .AddScoped<IRequestProcessor<RevocationValidatedRequest, Unit>, RevocationRequestProcessor>();
    }

    internal static IServiceCollection AddToken(this IServiceCollection services)
    {
        services
            .AddScoped<IRequestAccessor<TokenRequest>, TokenRequestAccessor>()
            .AddKeyedScoped<IEndpointHandler, TokenEndpointHandler>("Token")
            .AddSingleton<IEndpointModule, TokenEndpointModule>();

        services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, RefreshTokenRequestHandler>(GrantTypeConstants.RefreshToken)
            .AddScoped<IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse>, RefreshTokenRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>, RefreshTokenRequestValidator>();

        services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, AuthorizationCodeRequestHandler>(GrantTypeConstants.AuthorizationCode)
            .AddScoped<IRequestProcessor<AuthorizationCodeValidatedRequest, TokenResponse>, AuthorizationCodeRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>, AuthorizationCodeRequestValidator>();

        services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, ClientCredentialsRequestHandler>(GrantTypeConstants.ClientCredentials)
            .AddScoped<IRequestProcessor<ClientCredentialsValidatedRequest, TokenResponse>, ClientCredentialsRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>, ClientCredentialsRequestValidator>();

        return services;
    }
}