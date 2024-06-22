using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit.Repositories;

public class ClientRepositoryTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task DoesResourcesExist_ClientForResourceAndScope_True()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();
        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://localhost:5000"
        };
        client.Scopes.Add(await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(client);

        // Act
        var doesExist = await clientRepository.DoesResourcesExist([client.ClientUri], [ScopeConstants.OpenId], CancellationToken.None);

        // Assert
        Assert.True(doesExist);
    }

    [Fact]
    public async Task DoesResourcesExist_NoClientForResource_False()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        // Act
        var doesExist = await clientRepository.DoesResourcesExist(["https://localhost:5000"], [ScopeConstants.OpenId], CancellationToken.None);
        
        // Assert
        Assert.False(doesExist);
    }
}