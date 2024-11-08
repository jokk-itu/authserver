using AuthServer.Constants;
using AuthServer.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;

public class IntrospectionIntegrationTest : BaseIntegrationTest
{
    public IntrospectionIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Introspection_ActiveToken_ExpectActive()
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
        var introspectionResponse = await IntrospectionEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithClientSecret(registerResponse.ClientSecret!)
            .WithToken(tokenResponse.AccessToken)
            .WithTokenTypeHint(TokenTypeConstants.AccessToken)
            .WithTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretBasic)
            .Post();

        // Arrange
        Assert.True(introspectionResponse.Active);
        Assert.Equal(weatherReadScope, introspectionResponse.Scope);
        Assert.Equal([weatherClient.ClientUri], introspectionResponse.Audience);
        Assert.Equal(registerResponse.ClientId, introspectionResponse.ClientId);
        Assert.Equal(TokenTypeConstants.AccessToken, introspectionResponse.TokenType);
        Assert.Null(introspectionResponse.Username);
        Assert.Equal(registerResponse.ClientId, introspectionResponse.Subject);
        Assert.Equal(DiscoveryDocument.Issuer, introspectionResponse.Issuer);
        Assert.NotNull(introspectionResponse.JwtId);
    }
}