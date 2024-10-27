using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class GetRegisterTest : BaseIntegrationTest
{
    public GetRegisterTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task GetRegister_DefaultValues_ExpectDefaultValuedClient()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"connect/register?client_id={client.Id}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", registrationToken) }
        };
        var httpClient = GetHttpClient();

        // Act
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        // Assert
        Assert.NotNull(registerResponse);
        Assert.Equal("webapp", registerResponse.ClientName);
        Assert.Equal(ApplicationTypeConstants.Web, registerResponse.ApplicationType);
        Assert.Equal(TokenEndpointAuthMethodConstants.ClientSecretBasic, registerResponse.TokenEndpointAuthMethod);
    }
}
