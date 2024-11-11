using AuthServer.Authorize;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorize;

public class AuthorizeRequestValidatorTest : BaseUnitTest
{
    public AuthorizeRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid_client_id")]
    public async Task Validate_InvalidClientId_ExpectInvalidClient(string clientId)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService <
                        IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var request = new AuthorizeRequest
        {
            ClientId = clientId
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_GivenRequestAndRequestUri_ExpectInvalidRequestAndRequestUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService <
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = "uri",
            RequestObject = "object"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRequestAndRequestUri, processResult);
    }
}