using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Tests.Core;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.RefreshTokenGrant;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RefreshTokenGrant;

public class RefreshTokenRequestProcessorTest : BaseUnitTest
{
    public RefreshTokenRequestProcessorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_ProcessRequest_ExpectTokenResponse()
    {
        // Arrange
        var accessTokenBuilder = new Mock<ITokenBuilder<GrantAccessTokenArguments>>();
        var idTokenBuilder = new Mock<ITokenBuilder<IdTokenArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(accessTokenBuilder);
            services.AddScopedMock(idTokenBuilder);
        });
        var refreshTokenProcessor =
            serviceProvider.GetRequiredService<IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            AccessTokenExpiration = 300
        };
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        await AddEntity(authorizationGrant);

        var weatherClient = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://weather.authserver.dk"
        };
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        weatherClient.Scopes.Add(openIdScope);
        await AddEntity(weatherClient);

        var request = new RefreshTokenValidatedRequest
        {
            AuthorizationGrantId = authorizationGrant.Id,
            ClientId = client.Id,
            Resource = [weatherClient.ClientUri],
            Scope = [ScopeConstants.OpenId]
        };

        accessTokenBuilder
            .Setup(x =>
                x.BuildToken(It.Is<GrantAccessTokenArguments>(
                    y => y.Scope == request.Scope && y.AuthorizationGrantId == authorizationGrant.Id && y.Resource == request.Resource)
                    , CancellationToken.None))
            .ReturnsAsync("access_token")
            .Verifiable();

        idTokenBuilder
            .Setup(x =>
                x.BuildToken(It.Is<IdTokenArguments>(
                        y => y.Scope == request.Scope && y.AuthorizationGrantId == authorizationGrant.Id)
                    , CancellationToken.None))
            .ReturnsAsync("id_token")
            .Verifiable();

        // Act
        var tokenResponse = await refreshTokenProcessor.Process(request, CancellationToken.None);

        // Assert
        Assert.Equal("access_token", tokenResponse.AccessToken);
        Assert.Equal("id_token", tokenResponse.IdToken);
        Assert.Null(tokenResponse.RefreshToken);
        Assert.Equal(ScopeConstants.OpenId, tokenResponse.Scope);
        Assert.Equal(client.AccessTokenExpiration, tokenResponse.ExpiresIn);
    }
}