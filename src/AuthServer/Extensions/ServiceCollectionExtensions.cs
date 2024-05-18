﻿using AuthServer.Builders;
using AuthServer.Cache;
using AuthServer.Codes;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.Introspection;
using AuthServer.Options;
using AuthServer.Repositories;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.RequestAccessors.EndSession;
using AuthServer.RequestAccessors.Introspection;
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
using AuthServer.Userinfo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServer(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureDiscoveryDocumentOptions>();

        services
            .AddCoreServices()
            .AddEncoders()
            .AddBuilders()
            .AddDecoders()
            .AddUserinfo()
            .AddIntrospection()
            .AddRevocation()
            .AddAuthorizationCode()
            .AddRefreshToken()
            .AddClientCredentials()
            .AddRepositories()
            .AddCache()
            .AddRequestAccessors();

        services.AddJwtAuthentication();
        services.AddJwtAuthorization();

        return services;
    }

    internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .ConfigureOptions<ConfigureJwtBearerOptions>()
            .AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        return services;
    }

    internal static IServiceCollection AddJwtAuthorization(this IServiceCollection services)
    {
        return services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationConstants.Userinfo, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAssertion(context =>
                {
                    var scope = context.User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Scope)?.Value;
                    return scope is not null && scope.Split(' ').Contains(ScopeConstants.UserInfo);
                });
            });
            options.AddPolicy(AuthorizationConstants.ClientConfiguration, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ClientConfiguration);
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
            .AddScoped<IRequestAccessor<UserinfoRequest>, UserinfoRequestAccessor>();
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
            .AddScoped<IAuthorizeResponseBuilder, AuthorizeResponseBuilder>()
            .AddScoped<ITokenBuilder<LogoutTokenArguments>, LogoutTokenBuilder>()
            .AddScoped<ITokenBuilder<IdTokenArguments>, IdTokenBuilder>()
            .AddScoped<ITokenBuilder<ClientAccessTokenArguments>, ClientAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<GrantAccessTokenArguments>, GrantAccessTokenBuilder>()
            .AddScoped<ITokenBuilder<RefreshTokenArguments>, RefreshTokenBuilder>()
            .AddScoped<ITokenBuilder<RegistrationTokenArguments>, RegistrationTokenBuilder>()
            .AddScoped<ITokenBuilder<UserinfoTokenArguments>, UserinfoTokenBuilder>();
    }

    internal static IServiceCollection AddDecoders(this IServiceCollection services)
    {
        return services
            .AddScoped<ITokenDecoder<ServerIssuedTokenDecodeArguments>, ServerIssuedTokenDecoder>()
            .AddScoped<ITokenDecoder<ClientIssuedTokenDecodeArguments>, ClientIssuedTokenDecoder>();
    }

    internal static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpClient(HttpClientNameConstants.Client);

        return services
            .AddScoped<IClientAuthenticationService, ClientAuthenticationService>()
            .AddScoped<IClientJwkService, ClientJwkService>()
            .AddScoped<ITokenSecurityService, TokenSecurityService>();
    }

    internal static IServiceCollection AddEncoders(this IServiceCollection services)
    {
        services.AddDataProtection();
        return services
            .AddScoped<IAuthorizationCodeEncoder, AuthorizationCodeEncoder>();
    }

    internal static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddDbContext<IdentityContext>()
            .AddScoped<IClientRepository, ClientRepository>()
            .AddScoped<IConsentGrantRepository, ConsentGrantRepository>();
    }

    internal static IServiceCollection AddUserinfo(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestProcessor<UserinfoRequest, string>, UserinfoRequestProcessor>()
            .AddScoped<IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>, UserinfoRequestValidator>()
            .AddScoped<IUserinfoProcessor, UserinfoProcessor>();
    }

    internal static IServiceCollection AddIntrospection(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestProcessor<IntrospectionRequest, IntrospectionResponse>, IntrospectionRequestProcessor>()
            .AddScoped<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>,
                IntrospectionRequestValidator>()
            .AddScoped<ITokenIntrospection, TokenIntrospection>();
    }

    internal static IServiceCollection AddRevocation(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestProcessor<RevocationRequest, Unit>, RevocationRequestProcessor>()
            .AddScoped<IRequestValidator<RevocationRequest, RevocationValidatedRequest>, RevocationRequestValidator>()
            .AddScoped<ITokenRevoker, TokenRevoker>();
    }

    internal static IServiceCollection AddClientCredentials(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestProcessor<TokenRequest, TokenResponse>, ClientCredentialsRequestProcessor>(
                GrantTypeConstants.ClientCredentials)
            .AddScoped<IClientCredentialsProcessor, ClientCredentialsProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>,
                ClientCredentialsValidator>();
    }

    internal static IServiceCollection AddAuthorizationCode(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestProcessor<TokenRequest, TokenResponse>, AuthorizationCodeRequestProcessor>(
                GrantTypeConstants.AuthorizationCode)
            .AddScoped<IAuthorizationCodeProcessor, AuthorizationCodeProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>,
                AuthorizationCodeValidator>();
    }

    internal static IServiceCollection AddRefreshToken(this IServiceCollection services)
    {
        return services
            .AddKeyedScoped<IRequestProcessor<TokenRequest, TokenResponse>, RefreshTokenRequestProcessor>(
                GrantTypeConstants.RefreshToken)
            .AddScoped<IRefreshTokenProcessor, RefreshTokenProcessor>()
            .AddScoped<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>, RefreshTokenValidator>();
    }
}