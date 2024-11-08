using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;

public class RevocationIntegrationTest : BaseIntegrationTest
{
    public RevocationIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Revocation_ActiveToken_ExpectRevoked()
    {
        // Arrange
        var weatherReadScope = await AddWeatherReadScope();
        var weatherClient = await AddWeatherClient();

        var registerResponse = await RegisterEndpointBuilder
            .WithClientName("worker-app")
            .WithGrantTypes([GrantTypeConstants.ClientCredentials])
            .WithScope([weatherReadScope])
            .WithRequireReferenceToken()
            .Post();

        var tokenResponse = await TokenEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithGrantType(GrantTypeConstants.ClientCredentials)
            .WithResource([weatherClient.ClientUri!])
            .WithScope([weatherReadScope])
            .Post();

        // Act
        await RevocationEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithToken(tokenResponse.AccessToken)
            .WithTokenTypeHint(TokenTypeConstants.AccessToken)
            .WithTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretBasic)
            .Post();

        // Arrange
        var token = await ServiceProvider.GetRequiredService<AuthorizationDbContext>()
            .Set<Token>().SingleAsync(x => x.Reference == tokenResponse.AccessToken);

        Assert.NotNull(token.RevokedAt);
    }
}