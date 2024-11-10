using AuthServer.Authentication.Models;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.RefreshTokenGrant;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RefreshTokenGrant;

public class RefreshTokenRequestValidatorTest : BaseUnitTest
{
    public RefreshTokenRequestValidatorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_EmptyGrantType_ExpectUnsupportedGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest();

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnsupportedGrantType, processResult);
    }

    [Fact]
    public async Task Validate_EmptyRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_EmptyResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = "token"
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
    }

    [Fact]
    public async Task Validate_NoClientAuthentication_ExpectMultipleOrNoneClientMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = "token",
            Resource = ["resource"]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.MultipleOrNoneClientMethod, processResult);
    }

    [Fact]
    public async Task Validate_InvalidClientAuthentication_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = "token",
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    "clientId",
                    "clientSecret")
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_InvalidJwtRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client);

        var jwtRefreshToken = JwtBuilder.GetRefreshToken(
            "invalid_audience", refreshToken.AuthorizationGrant.Id, refreshToken.Id.ToString());

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = jwtRefreshToken,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_ExpiredJwtRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client, -3600);

        var jwtRefreshToken = JwtBuilder.GetRefreshToken(
            client.Id, refreshToken.AuthorizationGrant.Id, refreshToken.Id.ToString());

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = jwtRefreshToken,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_RevokedJwtRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client);
        refreshToken.Revoke();
        await SaveChangesAsync();

        var jwtRefreshToken = JwtBuilder.GetRefreshToken(
            client.Id, refreshToken.AuthorizationGrant.Id, refreshToken.Id.ToString());

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = jwtRefreshToken,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_InvalidReferenceRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        
        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = "invalid_reference",
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_ExpiredReferenceRefreshToken_ExpectInvalidRefreshToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client, -3600);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_RevokedReferenceRefreshToken_ExpectInvalidRefreshToken()
    {
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client);
        refreshToken.Revoke();
        await SaveChangesAsync();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRefreshToken, processResult);
    }

    [Fact]
    public async Task Validate_UnauthorizedForRefreshTokenGrant_ExpectUnauthorizedForGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);

        var refreshToken = await GetRefreshToken(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForGrantType, processResult);
    }

    [Fact]
    public async Task Validate_NoConsent_ExpectConsentRequired()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        var refreshTokenGrantType = await GetGrantType(GrantTypeConstants.RefreshToken);
        client.GrantTypes.Add(refreshTokenGrantType);

        var refreshToken = await GetRefreshToken(client);
        client.ConsentGrants.Clear();
        await SaveChangesAsync();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.ConsentRequired, processResult);
    }

    [Fact]
    public async Task Validate_ConsentRequiredWithRequestScope_ExpectScopeExceedsConsentedScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        var refreshTokenGrantType = await GetGrantType(GrantTypeConstants.RefreshToken);
        client.GrantTypes.Add(refreshTokenGrantType);

        var refreshToken = await GetRefreshToken(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Scope = [ScopeConstants.Profile],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.ScopeExceedsConsentedScope, processResult);
    }

    [Fact]
    public async Task Validate_ConsentNotRequiredWithRequestScope_ExpectUnauthorizedForScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        client.RequireConsent = false;
        var refreshTokenGrantType = await GetGrantType(GrantTypeConstants.RefreshToken);
        client.GrantTypes.Add(refreshTokenGrantType);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);

        var refreshToken = await GetRefreshToken(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Scope = [ScopeConstants.Profile],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForScope, processResult);
    }

    [Fact]
    public async Task Validate_InvalidResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        client.RequireConsent = false;
        var refreshTokenGrantType = await GetGrantType(GrantTypeConstants.RefreshToken);
        client.GrantTypes.Add(refreshTokenGrantType);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);

        var refreshToken = await GetRefreshToken(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = refreshToken.Reference,
            Resource = ["invalid_resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
    }

    [Fact]
    public async Task Validate_JwtRefreshTokenAndConsentRequired_ExpectValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(16);
        var client = await GetClient(plainSecret);
        var refreshTokenGrantType = await GetGrantType(GrantTypeConstants.RefreshToken);
        client.GrantTypes.Add(refreshTokenGrantType);

        var refreshToken = await GetRefreshToken(client);

        var jwtRefreshToken = JwtBuilder.GetRefreshToken(
            client.Id, refreshToken.AuthorizationGrant.Id, refreshToken.Id.ToString());

        var weatherClient = await GetWeatherClient();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.RefreshToken,
            RefreshToken = jwtRefreshToken,
            Scope = [ScopeConstants.OpenId],
            Resource = [weatherClient.ClientUri!],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.True(processResult.IsSuccess);
        Assert.Equal(refreshToken.AuthorizationGrant.Id, processResult.Value!.AuthorizationGrantId);
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.Equal([weatherClient.ClientUri!], processResult.Value!.Resource);
        Assert.Equal([ScopeConstants.OpenId], processResult.Value!.Scope);
    }

    private async Task<Client> GetClient(string plainSecret)
    {
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);
        await AddEntity(client);
        return client;
    }

    private async Task<Client> GetWeatherClient()
    {
        var weatherClient = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://weahter.authserver.dk"
        };
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        weatherClient.Scopes.Add(openIdScope);
        await AddEntity(weatherClient);
        return weatherClient;
    }

    private async Task<RefreshToken> GetRefreshToken(Client client, int? expiration = null)
    {
        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        var refreshToken = new RefreshToken(authorizationGrant, client.Id, DiscoveryDocument.Issuer, ScopeConstants.OpenId, DateTime.UtcNow.AddSeconds(expiration ?? 3600));
        await AddEntity(refreshToken);

        var consentGrant = new ConsentGrant(subjectIdentifier, client);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        consentGrant.ConsentedScopes.Add(openIdScope);
        await AddEntity(consentGrant);

        return refreshToken;
    }
}