using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.TokenBuilders;

public class RegistrationTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task BuildToken_RegistrationToken_ExpectReference()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var registrationTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RegistrationTokenArguments>>();
        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        // Act
        var registrationToken = await registrationTokenBuilder.BuildToken(new RegistrationTokenArguments
        {
            ClientId = client.Id
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var token = IdentityContext.Set<RegistrationToken>().Include(x => x.Client).Single();
        Assert.Equal(registrationToken, token.Reference);
        Assert.Equal(client.Id, token.Client.Id);
        Assert.Equal(DiscoveryDocument.Issuer, token.Issuer);
        Assert.Equal(ScopeConstants.Register, token.Scope);
        Assert.Null(token.ExpiresAt);
        Assert.Equal(client.Id, token.Audience);
    }
}