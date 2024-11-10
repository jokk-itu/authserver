using AuthServer.Authentication.Models;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.ClientCredentialsGrant;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.ClientCredentialsGrant;

public class ClientCredentialsRequestValidatorTest : BaseUnitTest
{
    public ClientCredentialsRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_EmptyGrantType_ExpectUnsupportedGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var request = new TokenRequest();
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnsupportedGrantType, processResult);
    }

    [Fact]
    public async Task Validate_EmptyScope_ExpectInvalidScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidScope, processResult);
    }

    [Fact]
    public async Task Validate_EmptyResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
    }

    [Fact]
    public async Task Validate_NoClientAuthentication_ExpectMultipleOrNoneClientMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId],
            Resource = ["resource"]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.MultipleOrNoneClientMethod, processResult);
    }

    [Fact]
    public async Task Validate_InvalidClientAuthentication_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    "clientId",
                    "clientSecret")
            ]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_UnauthorizedForClientCredentials_ExpectUnauthorizedForGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var client = new Client("worker-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);
        await AddEntity(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForGrantType, processResult);
    }

    [Fact]
    public async Task Validate_UnauthorizedScope_ExpectUnauthorizedForScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var client = new Client("worker-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);
        var clientCredentialsGrant = await GetGrantType(GrantTypeConstants.ClientCredentials);
        client.GrantTypes.Add(clientCredentialsGrant);
        await AddEntity(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForScope, processResult);
    }

    [Fact]
    public async Task Validate_ResourceDoesNotExist_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var client = new Client("worker-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);

        var clientCredentialsGrant = await GetGrantType(GrantTypeConstants.ClientCredentials);
        client.GrantTypes.Add(clientCredentialsGrant);

        var scope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(scope);

        await AddEntity(client);

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = [ScopeConstants.OpenId],
            Resource = ["resource"],
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
    }

    [Fact]
    public async Task Validate_ValidatedRequest_ExpectValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>>();

        var client = new Client("worker-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);

        var clientCredentialsGrant = await GetGrantType(GrantTypeConstants.ClientCredentials);
        client.GrantTypes.Add(clientCredentialsGrant);

        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);

        await AddEntity(client);

        var weatherClient = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://weather.authserver.dk"
        };
        weatherClient.Scopes.Add(openIdScope);
        await AddEntity(weatherClient);

        var scope = new[] { ScopeConstants.OpenId };
        var resource = new[] { weatherClient.ClientUri };
        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.ClientCredentials,
            Scope = scope,
            Resource = resource,
            ClientAuthentications = [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    client.Id,
                    plainSecret)
            ]
        };
        
        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.Equal(scope, processResult.Value!.Scope);
        Assert.Equal(resource, processResult.Value!.Resource);
    }
}