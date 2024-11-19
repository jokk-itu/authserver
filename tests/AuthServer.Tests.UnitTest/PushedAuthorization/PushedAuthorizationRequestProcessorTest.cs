using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.DatabaseConfigurations;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.PushedAuthorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.PushedAuthorization;

public class PushedAuthorizationRequestProcessorTest : BaseUnitTest
{
    public PushedAuthorizationRequestProcessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_AddAuthorizeMessage_ExpectPushedAuthorizationResponse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider
            .GetRequiredService<IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequestUriExpiration = 60
        };
        await AddEntity(client);

        const string value = "value";
        var request = new PushedAuthorizationValidatedRequest
        {
            LoginHint = value,
            IdTokenHint = value,
            Prompt = value,
            Display = value,
            ResponseType = value,
            ResponseMode = value,
            CodeChallenge = value,
            CodeChallengeMethod = value,
            Scope = [value],
            AcrValues = [value],
            ClientId = client.Id,
            MaxAge = value,
            Nonce = value,
            State = value,
            RedirectUri = value
        };

        // Act
        var response = await processor.Process(request, CancellationToken.None);
        await SaveChangesAsync();

        // Assert
        Assert.Equal(request.ClientId, response.ClientId);
        Assert.Equal(client.RequestUriExpiration, response.ExpiresIn);

        var authorizeMessage = await IdentityContext.Set<AuthorizeMessage>().SingleAsync();
        Assert.Equal($"{RequestUriConstants.RequestUriPrefix}{authorizeMessage.Reference}", response.RequestUri);
    }
}