using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Register;
using AuthServer.RequestAccessors.Register;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

    [Fact]
    public async Task Validate_InvalidApplicationType_ExpectInvalidApplicationType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ApplicationType = "invalid_application_type"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidApplicationType, processResult);
    }

    [Fact]
    public async Task Validate_InvalidTokenEndpointAuthMethod_ExpectInvalidTokenEndpointAuthMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            TokenEndpointAuthMethod = "invalid_token_endpoint_auth_method"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidTokenEndpointAuthMethod, processResult);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("duplicate")]
    public async Task Validate_InvalidClientName_ExpectInvalidClientName(string? clientName)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var client = new Client("duplicate", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = clientName
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidClientName, processResult);
    }

    [Fact]
    public async Task Validate_InvalidGrantTypes_ExpectInvalidGrantTypes()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            GrantTypes = ["invalid_grant_type"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidGrantTypes, processResult);
    }

    [Fact]
    public async Task Validate_OnlyRefreshTokenGrantType_ExpectInvalidGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            GrantTypes = [GrantTypeConstants.RefreshToken]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidGrantTypes, processResult);
    }

    [Fact]
    public async Task Validate_InvalidScope_ExpectInvalidScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            Scope = ["invalid_scope"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidScope, processResult);
    }

    [Fact]
    public async Task Validate_InvalidResponseType_ExpectInvalidResponseTypes()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            ResponseTypes = ["invalid_response_type"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidResponseTypes, processResult);
    }

    [Fact]
    public async Task Validate_EmptyRedirectUris_ExpectInvalidRedirectUris()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            RedirectUris = ["invalid_redirect_uris"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidRedirectUris, processResult);
    }

    [Theory]
    [InlineData(ApplicationType.Web)]
    [InlineData(ApplicationType.Native)]
    public async Task Validate_InvalidRedirectUris_ExpectInvalidRedirectUris(ApplicationType applicationType)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            ApplicationType = applicationType.GetDescription(),
            RedirectUris = ["invalid_redirect_uri"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidRedirectUris, processResult);
    }

    [Theory]
    [InlineData(ApplicationType.Web)]
    [InlineData(ApplicationType.Native)]
    public async Task Validate_InvalidPostLogoutRedirectUris_ExpectPostLogoutRedirectUris(ApplicationType applicationType)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            ApplicationType = applicationType.GetDescription(),
            RedirectUris = ["https://webapp.authserver.dk/callback"],
            PostLogoutRedirectUris = ["invalid_post_logout_redirect_uri"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidPostLogoutRedirectUris, processResult);
    }

    [Fact]
    public async Task Validate_InvalidRequestUris_ExpectRequestUris()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            ApplicationType = ApplicationType.Web.GetDescription(),
            RedirectUris = ["https://webapp.authserver.dk/callback"],
            RequestUris = ["invalid_request_uri"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidRequestUris, processResult);
    }

    [Fact]
    public async Task Validate_NoSectorIdentifierUriAndMultipleRedirectUris_InvalidSectorIdentifier()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            RedirectUris = ["https://webapp.authserver.dk/callback", "https://webapp.authserver.dk/callback2"],
            SubjectType = SubjectType.Pairwise.GetDescription()
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidSectorIdentifierUri, processResult);
    }

    [Fact]
    public async Task Validate_InvalidSectorIdentifierUri_InvalidSectorIdentifier()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            RedirectUris = ["https://webapp.authserver.dk/callback"],
            SubjectType = SubjectType.Pairwise.GetDescription(),
            SectorIdentifierUri = "invalid_sector_identifier_uri"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidSectorIdentifierUri, processResult);
    }

    [Fact]
    public async Task Validate_NoRedirectUriInSectorDocument_ExpectInvalidSectorDocument()
    {
        // Arrange
        var clientSectorService = new Mock<IClientSectorService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(clientSectorService);
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RegisterRequest, RegisterValidatedRequest>>();

        var request = new RegisterRequest
        {
            Method = HttpMethod.Post,
            ClientName = "web-app",
            RedirectUris = ["https://webapp.authserver.dk/callback"],
            SubjectType = SubjectType.Pairwise.GetDescription(),
            SectorIdentifierUri = "https://webapp.authserver.dk/sector"
        };

        clientSectorService
            .Setup(x => x.ContainsSectorDocument(
                It.Is<Uri>(y => y.ToString() == request.SectorIdentifierUri),
                request.RedirectUris,
                CancellationToken.None))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(RegisterError.InvalidSectorDocument, processResult);
    }
}