using AuthServer.Authentication.Models;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Introspection;
using AuthServer.RequestAccessors.Introspection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Introspection;

public class IntrospectionRequestValidatorTest : BaseUnitTest
{
    public IntrospectionRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_InvalidTokenTypeHint_ExpectUnsupportedTokenType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();
        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications = [],
            Token = string.Empty,
            TokenTypeHint = "invalid_token_type_hint"
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.Equal(IntrospectionError.UnsupportedTokenType, processResult);
    }

    [Fact]
    public async Task Validate_NoTokenProvided_ExpectEmptyToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();
        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications = [],
            Token = string.Empty,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.Equal(IntrospectionError.EmptyToken, processResult);
    }

    [Fact]
    public async Task Validate_NoClientAuthentication_ExpectMultipleOrNoneClientMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();
        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications = [],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.Equal(IntrospectionError.MultipleOrNoneClientMethod, processResult);
    }

    [Fact]
    public async Task Validate_InvalidClientAuthenticationMethod_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();
        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications = [new ClientIdAuthentication("client_id")],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.Equal(IntrospectionError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_UnauthenticatedClient_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();
        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, "client_id", "client_secret")
            ],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.Equal(IntrospectionError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_IntrospectToken_ExpectIntrospectionValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashSecret);
        var openIdScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.OpenId);
        var profileScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.Profile);
        client.Scopes.Add(openIdScope);
        client.Scopes.Add(profileScope);

        var token = new ClientAccessToken(client, client.ClientUri, DiscoveryDocument.Issuer, $"{ScopeConstants.OpenId} {ScopeConstants.Address}",
            DateTime.UtcNow.AddHours(1));

        await AddEntity(token);

        var introspectionRequest = new IntrospectionRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainSecret)
            ],
            Token = token.Reference,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(introspectionRequest, CancellationToken.None);

        // Assert
        Assert.IsType<IntrospectionValidatedRequest>(processResult.Value);
        Assert.Equal(client.Id, processResult.Value.ClientId);
        Assert.Equal(token.Reference, processResult.Value.Token);
    }
}