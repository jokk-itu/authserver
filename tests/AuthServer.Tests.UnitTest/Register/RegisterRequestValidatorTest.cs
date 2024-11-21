using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Register;
using AuthServer.RequestAccessors.Register;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Register;

public class RegisterRequestValidatorTest : BaseUnitTest
{
    public RegisterRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Validate_ClientIdIsNull_ExpectInvalidClientId(string httpMethod)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator =
            serviceProvider.GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Parse(httpMethod)
        };

        // Act
        var processError = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidClientId, processError);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Validate_RegistrationAccessTokenIsNull_ExpectInvalidRegistrationAccessToken(string httpMethod)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator =
            serviceProvider.GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Parse(httpMethod),
            ClientId = "clientId"
        };

        // Act
        var processError = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidRegistrationAccessToken, processError);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Validate_GivenRegistrationAccessTokenIsInactive_ExpectInvalidRegistrationAccessToken(string httpMethod)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator =
            serviceProvider.GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Parse(httpMethod),
            ClientId = "clientId",
            RegistrationAccessToken = "inactiveToken"
        };

        // Act
        var processError = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidRegistrationAccessToken, processError);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Validate_ClientIdDoesNotMatchToken_ExpectMismatchingClientId(string httpMethod)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator =
            serviceProvider.GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var registrationToken = new RegistrationToken(client, "aud", "iss", ScopeConstants.Register);
        await AddEntity(registrationToken);

        var request = new RegisterRequest
        {
            Method = HttpMethod.Parse(httpMethod),
            ClientId = "clientId",
            RegistrationAccessToken = registrationToken.Reference
        };

        // Act
        var processError = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.MismatchingClientId, processError);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task Validate_ValidRequestForDeleteAndGet_ExpectRegisterValidatedRequest(string httpMethod)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator =
            serviceProvider.GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var registrationToken = new RegistrationToken(client, "aud", "iss", ScopeConstants.Register);
        await AddEntity(registrationToken);

        var request = new RegisterRequest
        {
            Method = HttpMethod.Parse(httpMethod),
            ClientId = client.Id,
            RegistrationAccessToken = registrationToken.Reference
        };

        // Act
        var processError = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.Method, processError.Value!.Method);
        Assert.Equal(request.ClientId, processError.Value!.ClientId);
        Assert.Equal(request.RegistrationAccessToken, processError.Value!.RegistrationAccessToken);
    }
}