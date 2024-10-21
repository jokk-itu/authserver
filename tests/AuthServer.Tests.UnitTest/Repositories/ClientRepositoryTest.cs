using System.Text.Json;
using AuthServer.Authorization;
using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Repositories;

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

    [Fact]
    public async Task GetAuthorizeDto_ReferenceExists_ExpectAuthorizeRequestDto()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var dto = new AuthorizeRequestDto
        {
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(32),
            Display = DisplayConstants.Page,
            AcrValues = ["pwd"],
            CodeChallenge = CryptographyHelper.GetRandomString(32),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            IdTokenHint = string.Empty,
            Prompt = PromptConstants.Login,
            RedirectUri = string.Empty,
            LoginHint = string.Empty,
            MaxAge = string.Empty,
            Scope = [ScopeConstants.OpenId],
            ResponseMode = string.Empty,
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(32)
        };
        
        var authorizeMessage = new AuthorizeMessage(
            JsonSerializer.Serialize(dto),
            DateTime.UtcNow.AddSeconds(90),
            client);

        await AddEntity(authorizeMessage);

        // Act
        var authorizeDto = await clientRepository.GetAuthorizeDto(authorizeMessage.Reference, client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(authorizeDto);
    }

    [Fact]
    public async Task GetAuthorizeDto_ReferenceExpired_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var dto = new AuthorizeRequestDto
        {
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(32),
            Display = DisplayConstants.Page,
            AcrValues = ["pwd"],
            CodeChallenge = CryptographyHelper.GetRandomString(32),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            IdTokenHint = string.Empty,
            Prompt = PromptConstants.Login,
            RedirectUri = string.Empty,
            LoginHint = string.Empty,
            MaxAge = string.Empty,
            Scope = [ScopeConstants.OpenId],
            ResponseMode = string.Empty,
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(32)
        };

        var authorizeMessage = new AuthorizeMessage(
            JsonSerializer.Serialize(dto),
            DateTime.UtcNow.AddSeconds(-90),
            client);

        await AddEntity(authorizeMessage);

        // Act
        var authorizeDto = await clientRepository.GetAuthorizeDto(authorizeMessage.Reference, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(authorizeDto);
    }

    [Fact]
    public async Task GetAuthorizeDto_ReferenceDoesNotExist_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        // Act
        var authorizeDto = await clientRepository.GetAuthorizeDto(
            CryptographyHelper.GetRandomString(32),
            CryptographyHelper.GetRandomString(32),
            CancellationToken.None);

        // Assert
        Assert.Null(authorizeDto);
    }

    [Fact]
    public async Task AddAuthorizeMessage_ExpectAuthorizeMessage()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequestUriExpiration = 300
        };
        await AddEntity(client);
        var dto = new AuthorizeRequestDto
        {
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(32),
            Display = DisplayConstants.Page,
            AcrValues = ["pwd"],
            CodeChallenge = CryptographyHelper.GetRandomString(32),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            IdTokenHint = string.Empty,
            Prompt = PromptConstants.Login,
            RedirectUri = string.Empty,
            LoginHint = string.Empty,
            MaxAge = string.Empty,
            Scope = [ScopeConstants.OpenId],
            ResponseMode = string.Empty,
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(32)
        };

        // Act
        var authorizeMessage = await clientRepository.AddAuthorizeMessage(dto, CancellationToken.None);

        // Assert
        Assert.NotNull(authorizeMessage);
    }

    [Fact]
    public async Task RedeemAuthorizeMessage()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var dto = new AuthorizeRequestDto
        {
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(32),
            Display = DisplayConstants.Page,
            AcrValues = ["pwd"],
            CodeChallenge = CryptographyHelper.GetRandomString(32),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            IdTokenHint = string.Empty,
            Prompt = PromptConstants.Login,
            RedirectUri = string.Empty,
            LoginHint = string.Empty,
            MaxAge = string.Empty,
            Scope = [ScopeConstants.OpenId],
            ResponseMode = string.Empty,
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(32)
        };

        var authorizeMessage = new AuthorizeMessage(
            JsonSerializer.Serialize(dto),
            DateTime.UtcNow.AddSeconds(-90),
            client);

        await AddEntity(authorizeMessage);

        // Act
        await clientRepository.RedeemAuthorizeMessage(authorizeMessage.Reference, CancellationToken.None);

        // Assert
        Assert.NotNull(authorizeMessage.RedeemedAt);
    }
}