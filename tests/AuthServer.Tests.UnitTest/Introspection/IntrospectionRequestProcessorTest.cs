using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Introspection;
using AuthServer.Tests.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Introspection;
public class IntrospectionRequestProcessorTest : BaseUnitTest
{
    public IntrospectionRequestProcessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_InvalidToken_ExpectActiveIsFalse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        await AddEntity(client);

        var introspectionValidatedRequest = new IntrospectionValidatedRequest
        {
            Token = "invalid_token",
            Scope = [ScopeConstants.OpenId],
            ClientId = client.Id
        };

        // Act
        var introspectionResponse = await processor.Process(introspectionValidatedRequest, CancellationToken.None);

        // Assert
        Assert.False(introspectionResponse.Active);
    }

    [Fact]
    public async Task Process_ExpiredToken_ExpectActiveIsFalse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var openIdScope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);
        var token = new ClientAccessToken(client, client.ClientUri, DiscoveryDocument.Issuer, ScopeConstants.OpenId, DateTime.UtcNow.AddHours(-1));
        
        await AddEntity(token);

        var introspectionValidatedRequest = new IntrospectionValidatedRequest
        {
            Token = token.Reference,
            Scope = [ScopeConstants.OpenId],
            ClientId = client.Id
        };

        // Act
        var introspectionResponse = await processor.Process(introspectionValidatedRequest, CancellationToken.None);

        // Assert
        Assert.False(introspectionResponse.Active);
    }

    [Fact]
    public async Task Process_RevokedToken_ExpectActiveIsFalse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var openIdScope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);
        var token = new ClientAccessToken(client, client.ClientUri, DiscoveryDocument.Issuer, ScopeConstants.OpenId, DateTime.UtcNow.AddHours(1));
        token.Revoke();
        await AddEntity(token);

        var introspectionValidatedRequest = new IntrospectionValidatedRequest
        {
            Token = token.Reference,
            Scope = [ScopeConstants.OpenId],
            ClientId = client.Id
        };

        // Act
        var introspectionResponse = await processor.Process(introspectionValidatedRequest, CancellationToken.None);

        // Assert
        Assert.False(introspectionResponse.Active);
    }

    [Fact]
    public async Task Process_InsufficientScopeForClient_ExpectActiveIsFalse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var token = new ClientAccessToken(client, client.ClientUri, DiscoveryDocument.Issuer, ScopeConstants.OpenId, DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var introspectionValidatedRequest = new IntrospectionValidatedRequest
        {
            Token = token.Reference,
            Scope = [],
            ClientId = client.Id
        };

        // Act
        var introspectionResponse = await processor.Process(introspectionValidatedRequest, CancellationToken.None);

        // Assert
        Assert.False(introspectionResponse.Active);
    }

    [Fact]
    public async Task Process_ActiveToken_ExpectIsActiveIsTrue()
    {
        // Arrange
        var userClaimServiceMock = new Mock<IUserClaimService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(userClaimServiceMock);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>>();

        var subjectIdentifier = new SubjectIdentifier();
        const string username = "JohnDoe";
        userClaimServiceMock
            .Setup(x => x.GetUsername(subjectIdentifier.Id, CancellationToken.None))
            .ReturnsAsync(username)
            .Verifiable();

        var session = new Session(subjectIdentifier);

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var openidScope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
        client.Scopes.Add(openidScope);

        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        
        var token = new GrantAccessToken(authorizationGrant, client.ClientUri, DiscoveryDocument.Issuer, ScopeConstants.OpenId, DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var introspectionValidatedRequest = new IntrospectionValidatedRequest
        {
            Token = token.Reference,
            Scope = [ScopeConstants.OpenId],
            ClientId = client.Id
        };

        // Act
        var introspectionResponse = await processor.Process(introspectionValidatedRequest, CancellationToken.None);

        // Assert
        Assert.True(introspectionResponse.Active);
        Assert.Equal(token.Id.ToString(), introspectionResponse.JwtId);
        Assert.Equal(client.Id, introspectionResponse.ClientId);
        Assert.Equal(token.ExpiresAt!.Value.ToUnixTimeSeconds(), introspectionResponse.ExpiresAt);
        Assert.Equal(DiscoveryDocument.Issuer, introspectionResponse.Issuer);
        Assert.Equal(token.Audience, introspectionResponse.Audience.Single());
        Assert.Equal(token.IssuedAt.ToUnixTimeSeconds(),  introspectionResponse.IssuedAt!.Value);
        Assert.Equal(token.NotBefore.ToUnixTimeSeconds(), introspectionResponse.NotBefore!.Value);
        Assert.Equal(token.Scope, introspectionResponse.Scope);
        Assert.Equal(subjectIdentifier.Id, introspectionResponse.Subject);
        Assert.Equal(token.TokenType.GetDescription(), introspectionResponse.TokenType);
        Assert.Equal(username, introspectionResponse.Username);
        userClaimServiceMock.Verify();
    }
}
