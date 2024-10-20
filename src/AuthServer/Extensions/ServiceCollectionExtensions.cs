using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.OAuthToken;
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
        Action<DbContextOptionsBuilder> databaseConfigurator)
    {
        services
            .AddClientServices()
            .AddCodeEncoders()
            .AddBuilders()
            .AddDecoders()
            .AddDataStore(databaseConfigurator)
            .AddOptions()
            .AddPushedAuthorization()
            .AddRegister()
            .AddEndSession()
            .AddAuthorize()
            .AddUserinfo()
            .AddIntrospection()
            .AddRevocation()
            .AddAuthorizationCode()
            .AddRefreshToken()
            .AddClientCredentials()
            .AddRepositories()
            .AddCache()
            .AddRequestAccessors();

        services.AddAuthServerAuthentication();
        services.AddAuthServerAuthorization();

        services.AddSingleton<IMetricService, MetricService>();
        services.AddHttpContextAccessor();

        return services;
    }

    internal static IServiceCollection AddOptions(this IServiceCollection services)
    {
        return services
            .ConfigureOptions<PostConfigureDiscoveryDocumentOptions>()
            .ConfigureOptions<ValidateDiscoveryDocumentOptions>()
            .ConfigureOptions<ValidateJwksDocument>()
            .ConfigureOptions<ValidateUserInteractionOptions>();
    }

    internal static IServiceCollection AddDataStore(this IServiceCollection services,
        Action<DbContextOptionsBuilder> databaseConfigurator)
    {
        return services
            .AddDbContext<AuthorizationDbContext>(databaseConfigurator)
            .AddScoped<IUnitOfWork, UnitOfWork>();
    }

    internal static IServiceCollection AddAuthServerAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<OAuthTokenAuthenticationOptions, OAuthTokenAuthenticationHandler>(
                OAuthTokenAuthenticationDefaults.AuthenticationScheme, null);

        return services;
    }

    internal static IServiceCollection AddAuthServerAuthorization(this IServiceCollection services)
    {
        return services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationConstants.Userinfo, policy =>
            {
                policy.AddAuthenticationSchemes(OAuthTokenAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAssertion(context =>
                {
                    var scope = context.User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Scope)?.Value;
                    return scope is not null && scope.Split(' ').Contains(ScopeConstants.UserInfo);
                });
            });
            options.AddPolicy(AuthorizationConstants.Register, policy =>
            {
                policy.AddAuthenticationSchemes(OAuthTokenAuthenticationDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.Register);
            });
        });
    }

    internal static IServiceCollection AddRequestAccessors(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestAccessor<AuthorizeRequest>, AuthorizeRequestAccessor>()
            .AddScoped<IRequestAccessor<EndSessionRequest>, EndSessionRequestAccessor>()
            .AddScoped<IRequestAccessor<IntrospectionRequest>, IntrospectionRequestAccessor>()
            .AddScoped<IRequestAccessor<RevocationRequest>, RevocationRequestAccessor>()
            .AddScoped<IRequestAccessor<RegisterRequest>, RegisterRequestAccessor>()
            .AddScoped<IRequestAccessor<TokenRequest>, TokenRequestAccessor>()
            .AddScoped<IRequestAccessor<UserinfoRequest>, UserinfoRequestAccessor>()
            .AddScoped<IRequestAccessor<PushedAuthorizationRequest>, PushedAuthorizationRequestAccessor>();
    }

    internal static IServiceCollection AddCache(this IServiceCollection services)
    {
        return services
            .AddScoped<ICachedClientStore, CachedClientStore>()
            .AddScoped<ITokenReplayCache, TokenReplayCache>();
    }

    internal static IServiceCollection AddBuilders(this IServiceCollection services)
    {
        return services
            .AddScoped<ITokenBuilder<LogoutTokenArguments>, LogoutTokenBuilder>()
            .AddScoped<ITokenBuilder<IdTokenArguments>, IdTokenBuilder>()
            .AddScoped<ITokenBuilder<ClientAccessTokenArguments>, ClientAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<GrantAccessTokenArguments>, GrantAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<RefreshTokenArguments>, RefreshTokenBuilder>()
            .AddScoped<ITokenBuilder<RegistrationTokenArguments>, RegistrationTokenBuilder>()
            .AddScoped<ITokenBuilder<UserinfoTokenArguments>, UserinfoTokenBuilder>()
            .AddScoped<ITokenSecurityService, TokenSecurityService>();
    }

    internal static IServiceCollection AddDecoders(this IServiceCollection services)
    {
        return services
            .AddScoped<ITokenDecoder<ServerIssuedTokenDecodeArguments>, ServerIssuedTokenDecoder>()
            .AddScoped<ITokenDecoder<ClientIssuedTokenDecodeArguments>, ClientIssuedTokenDecoder>();
    }

    internal static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        services.AddHttpClient(HttpClientNameConstants.Client);

        return services
            .AddScoped<IClientAuthenticationService, ClientAuthenticationService>()
            .AddScoped<IClientJwkService, ClientJwkService>()
            .AddScoped<IClientSectorService, ClientSectorService>();
    }

    internal static IServiceCollection AddCodeEncoders(this IServiceCollection services)
    {
        services.AddDataProtection();
        return services
            .AddScoped<IAuthorizationCodeEncoder, AuthorizationCodeEncoder>();
    }

    internal static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IClientRepository, ClientRepository>()
            .AddScoped<IConsentGrantRepository, ConsentGrantRepository>()
            .AddScoped<IAuthorizationGrantRepository, AuthorizationGrantRepository>()
            .AddScoped<ITokenRepository, TokenRepository>()
            .AddScoped<INonceRepository, NonceRepository>();
    }

    internal static IServiceCollection AddPushedAuthorization(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<PushedAuthorizationRequest, PushedAuthorizationResponse>,
                PushedAuthorizationRequestHandler>()
            .AddScoped<IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>,
                PushedAuthorizationRequestProcessor>()
            .AddScoped<IRequestValidator<PushedAuthorizationRequest, PushedAuthorizationValidatedRequest>,
                PushedAuthorizationRequestValidator>();
    }

    internal static IServiceCollection AddRegister(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<RegisterRequest, ProcessResult<RegisterResponse, Unit>>,
                RegisterRequestHandler>()
            .AddScoped<IRequestValidator<RegisterRequest, RegisterValidatedRequest>, RegisterRequestValidator>()
            .AddScoped<IRequestProcessor<RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>>,
                RegisterRequestProcessor>();
    }

    internal static IServiceCollection AddEndSession(this IServiceCollection services)
    {
        return services
            .AddScoped<IEndSessionUserAccessor, EndSessionUserAccessor>()
            .AddScoped<IRequestHandler<EndSessionRequest, Unit>, EndSessionRequestHandler>()
            .AddScoped<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>, EndSessionRequestValidator>()
            .AddScoped<IRequestProcessor<EndSessionValidatedRequest, Unit>, EndSessionRequestProcessor>();
    }

    internal static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        return services
            .AddScoped<IAuthorizeService, AuthorizeService>()
            .AddScoped<IAuthorizeInteractionService, AuthorizeInteractionService>()
            .AddScoped<IAuthorizeResponseBuilder, AuthorizeResponseBuilder>()
            .AddScoped<IAuthorizeUserAccessor, AuthorizeUserAccessor>()
            .AddScoped<IAuthorizeRequestParameterService, AuthorizeRequestParameterService>()
            .AddScoped<IRequestHandler<AuthorizeRequest, string>, AuthorizeRequestHandler>()
            .AddScoped<IRequestProcessor<AuthorizeValidatedRequest, string>, AuthorizeRequestProcessor>()
            .AddScoped<IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>, AuthorizeRequestValidator>();
    }

    internal static IServiceCollection AddUserinfo(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<UserinfoRequest, string>, UserinfoRequestHandler>()
            .AddScoped<IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>, UserinfoRequestValidator>()
            .AddScoped<IRequestProcessor<UserinfoValidatedRequest, string>, UserinfoRequestProcessor>();
    }

    internal static IServiceCollection AddIntrospection(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<IntrospectionRequest, IntrospectionResponse>, IntrospectionRequestHandler>()
            .AddScoped<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>,
                IntrospectionRequestValidator>()
            .AddScoped<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>,
                IntrospectionRequestProcessor>();
    }

    internal static IServiceCollection AddRevocation(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<RevocationRequest, Unit>, RevocationRequestHandler>()
            .AddScoped<IRequestValidator<RevocationRequest, RevocationValidatedRequest>, RevocationRequestValidator>()
            .AddScoped<IRequestProcessor<RevocationValidatedRequest, Unit>, RevocationRequestProcessor>();
    }

    internal static IServiceCollection AddClientCredentials(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, ClientCredentialsRequestHandler>(
                GrantTypeConstants.ClientCredentials)
            .AddScoped<IRequestProcessor<ClientCredentialsValidatedRequest, TokenResponse>,
                ClientCredentialsRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>,
                ClientCredentialsValidator>();
    }

    internal static IServiceCollection AddAuthorizationCode(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, AuthorizationCodeRequestHandler>(
                GrantTypeConstants.AuthorizationCode)
            .AddScoped<IRequestProcessor<AuthorizationCodeValidatedRequest, TokenResponse>,
                AuthorizationCodeRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>,
                AuthorizationCodeValidator>();
    }

    internal static IServiceCollection AddRefreshToken(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestHandler<TokenRequest, TokenResponse>, RefreshTokenRequestHandler>(GrantTypeConstants
                .RefreshToken)
            .AddScoped<IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse>, RefreshTokenRequestProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>, RefreshTokenValidator>();
    }
}