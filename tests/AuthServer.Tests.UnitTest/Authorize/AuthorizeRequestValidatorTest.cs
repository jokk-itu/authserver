using AuthServer.Authorization.Abstractions;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

    [Fact]
    public async Task Validate_RequireSignedRequestWithEmptyRequestAndRequestUri_ExpectRequestOrRequestUriRequiredAsRequestObject()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireSignedRequestObject = true
        };
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.RequestOrRequestUriRequiredAsRequestObject, processResult);
    }

    [Fact]
    public async Task Validate_RequirePushedAuthorizationWithEmptyRequestUri_ExpectRequestUriRequiredAsPushedAuthorizationRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequirePushedAuthorizationRequests = true
        };
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.RequestUriRequiredAsPushedAuthorizationRequest, processResult);
    }

    [Fact]
    public async Task Validate_InvalidRequestUriFromPushedAuthorization_ExpectInvalidOrExpireRequestUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = $"{RequestUriConstants.RequestUriPrefix}invalid_value"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidOrExpiredRequestUri, processResult);
    }

    [Fact]
    public async Task Validate_ValidRequestUriFromPushedAuthorization_ExpectAuthorizeValidatedRequest()
    {
        // Arrange
        var secureRequestService = new Mock<ISecureRequestService>();
        var authorizeInteractionService = new Mock<IAuthorizeInteractionService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(secureRequestService);
            services.AddScopedMock(authorizeInteractionService);
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = $"{RequestUriConstants.RequestUriPrefix}invalid_value"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidOrExpiredRequestUri, processResult);
    }
}