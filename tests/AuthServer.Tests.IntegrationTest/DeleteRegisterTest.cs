using System.Net;
using System.Net.Http.Headers;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class DeleteRegisterTest : BaseIntegrationTest
{
    public DeleteRegisterTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task DeleteRegister_DeleteClient_ExpectDeleted()
    {
        // Arrange
        var databaseContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        databaseContext.Add(client);
        await databaseContext.SaveChangesAsync();

        var registrationTokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder<RegistrationTokenArguments>>();
        var registrationToken = await registrationTokenBuilder.BuildToken(new RegistrationTokenArguments
        {
            ClientId = client.Id
        }, CancellationToken.None);
        await databaseContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"connect/register?client_id={client.Id}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", registrationToken) }
        };
        var httpClient = GetHttpClient();

        // Act
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await databaseContext.Set<Client>().SingleOrDefaultAsync(c => c.Id == client.Id));
    }
}
