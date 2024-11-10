using Microsoft.AspNetCore.Mvc.Testing;
using AuthServer.Constants;
using AuthServer.Enums;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using Xunit.Abstractions;
using AuthServer.Authorize.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Tests.IntegrationTest;
public class TokenIntegrationTest : BaseIntegrationTest
{
    public TokenIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Token_AuthorizationCodeGrant_ExpectTokens()
    {
        // Arrange
        var weatherReadScope = await AddWeatherReadScope();
        var userinfoScope = GetUserinfoScope(); 
        var weatherClient = await AddWeatherClient();
        var identityProviderClient = await AddIdentityProviderClient();

        var registerResponse = await RegisterEndpointBuilder
            .WithClientName("web-app")
            .WithRedirectUris(["https://webapp.authserver.dk/callback"])
            .WithGrantTypes([GrantTypeConstants.AuthorizationCode])
            .WithScope([userinfoScope, weatherReadScope, ScopeConstants.OpenId])
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [weatherReadScope, userinfoScope, ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithAuthorizeUser()
            .WithCodeChallenge(proofKeyForCodeExchange.CodeChallenge)
            .WithScope([weatherReadScope, userinfoScope, ScopeConstants.OpenId])
            .Get();

        // Act
        var tokenResponse = await TokenEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithCode(authorizeResponse.Code!)
            .WithCodeVerifier(proofKeyForCodeExchange.CodeVerifier)
            .WithResource([weatherClient.ClientUri!, identityProviderClient.ClientUri!])
            .WithGrantType(GrantTypeConstants.AuthorizationCode)
            .Post();

        // Assert
        Assert.NotNull(tokenResponse);
        Assert.Equal($"{weatherReadScope} {userinfoScope} {ScopeConstants.OpenId}", tokenResponse.Scope);
        Assert.Equal("Bearer", tokenResponse.TokenType);
        Assert.Null(tokenResponse.RefreshToken);
        Assert.NotNull(tokenResponse.IdToken);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.Equal(registerResponse.AccessTokenExpiration, tokenResponse.ExpiresIn);
    }

    [Fact]
    public async Task Token_RefreshTokenGrant_ExpectTokens()
    {
        // Arrange
        var scope = await AddWeatherReadScope();
        var weatherClient = await AddWeatherClient();

        var registerResponse = await RegisterEndpointBuilder
            .WithClientName("web-app")
            .WithRedirectUris(["https://webapp.authserver.dk/callback"])
            .WithGrantTypes([GrantTypeConstants.AuthorizationCode, GrantTypeConstants.RefreshToken])
            .WithScope([scope, ScopeConstants.OpenId])
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [scope, ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithAuthorizeUser()
            .WithCodeChallenge(proofKeyForCodeExchange.CodeChallenge)
            .WithScope([scope, ScopeConstants.OpenId])
            .Get();

        var tokenResponse = await TokenEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithCode(authorizeResponse.Code!)
            .WithCodeVerifier(proofKeyForCodeExchange.CodeVerifier)
            .WithResource([weatherClient.ClientUri!])
            .WithGrantType(GrantTypeConstants.AuthorizationCode)
            .Post();

        var refreshResponse = await TokenEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithRefreshToken(tokenResponse.RefreshToken!)
            .WithGrantType(GrantTypeConstants.RefreshToken)
            .WithResource([weatherClient.ClientUri!])
            .WithScope([scope])
            .Post();

        // Assert
        Assert.NotNull(refreshResponse);
        Assert.Equal(scope, refreshResponse.Scope);
        Assert.Equal("Bearer", refreshResponse.TokenType);
        Assert.Null(refreshResponse.RefreshToken);
        Assert.NotEqual(tokenResponse.RefreshToken, refreshResponse.RefreshToken);
        Assert.NotNull(refreshResponse.IdToken);
        Assert.NotNull(refreshResponse.AccessToken);
        Assert.Equal(registerResponse.AccessTokenExpiration, refreshResponse.ExpiresIn);
    }

    [Fact]
    public async Task Token_ClientCredentialsGrantWithPrivateKeyJwt_ExpectAccessToken()
    {
        // Arrange
        var scope = await AddWeatherReadScope();
        var weatherClient = await AddWeatherClient();
        var jwks = ClientJwkBuilder.GetClientJwks();

        var registerResponse = await RegisterEndpointBuilder
            .WithJwks(jwks.PublicJwks)
            .WithGrantTypes([GrantTypeConstants.ClientCredentials])
            .WithTokenEndpointAuthMethod(TokenEndpointAuthMethod.PrivateKeyJwt)
            .WithScope([scope])
            .WithClientName("worker-app")
            .Post();
        
        var clientAssertion = JwtBuilder.GetPrivateKeyJwt(registerResponse.ClientId, jwks.PrivateJwks, ClientTokenAudience.TokenEndpoint);
        
        // Act
        var tokenResponse = await TokenEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientAssertion(clientAssertion)
            .WithTokenEndpointAuthMethod(TokenEndpointAuthMethod.PrivateKeyJwt)
            .WithGrantType(GrantTypeConstants.ClientCredentials)
            .WithScope([scope])
            .WithResource([weatherClient.ClientUri!])
            .Post();

        // Assert
        Assert.NotNull(tokenResponse);
        Assert.Equal(scope, tokenResponse.Scope);
        Assert.Equal("Bearer", tokenResponse.TokenType);
        Assert.Null(tokenResponse.RefreshToken);
        Assert.Null(tokenResponse.IdToken);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.Equal(registerResponse.AccessTokenExpiration, tokenResponse.ExpiresIn);
    }
}
