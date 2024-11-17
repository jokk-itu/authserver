using AuthServer.Authorization;
using AuthServer.Authorization.Abstractions;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;
using ProofKeyForCodeExchangeHelper = AuthServer.Tests.Core.ProofKeyForCodeExchangeHelper;

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

        const string requestUri = $"{RequestUriConstants.RequestUriPrefix}valid_value";
        const string subjectIdentifier = "subjectIdentifier";

        var authorizeRequestDto = new AuthorizeRequestDto
        {
            ResponseMode = ResponseModeConstants.FormPost,
            CodeChallenge = CryptographyHelper.GetRandomString(16),
            Scope = [ScopeConstants.OpenId],
            AcrValues = [LevelOfAssuranceLow],
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(16),
            RedirectUri = "https://webapp.authserver.dk/callback"
        };

        secureRequestService
            .Setup(x => x.GetRequestByPushedRequest(requestUri, client.Id, CancellationToken.None))
            .ReturnsAsync(authorizeRequestDto)
            .Verifiable();

        authorizeInteractionService
            .Setup(x =>
                x.GetInteractionResult(It.Is<AuthorizeRequest>(y =>
                    y.ResponseMode == authorizeRequestDto.ResponseMode &&
                    y.CodeChallenge == authorizeRequestDto.CodeChallenge &&
                    y.Scope == authorizeRequestDto.Scope &&
                    y.AcrValues == authorizeRequestDto.AcrValues &&
                    y.ClientId == authorizeRequestDto.ClientId &&
                    y.Nonce == authorizeRequestDto.Nonce &&
                    y.RedirectUri == authorizeRequestDto.RedirectUri &&
                    y.RequestUri == requestUri), CancellationToken.None))
            .ReturnsAsync(InteractionResult.Success(subjectIdentifier))
            .Verifiable();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = requestUri
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        secureRequestService.Verify();
        authorizeInteractionService.Verify();

        Assert.Equal(subjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(authorizeRequestDto.ResponseMode, processResult.Value!.ResponseMode);
        Assert.Equal(authorizeRequestDto.CodeChallenge, processResult.Value!.CodeChallenge);
        Assert.Equal(authorizeRequestDto.Scope, processResult.Value!.Scope);
        Assert.Equal(authorizeRequestDto.AcrValues, processResult.Value!.AcrValues);
        Assert.Equal(authorizeRequestDto.ClientId, processResult.Value!.ClientId);
        Assert.Equal(authorizeRequestDto.Nonce, processResult.Value!.Nonce);
        Assert.Equal(authorizeRequestDto.RedirectUri, processResult.Value!.RedirectUri);
        Assert.Equal(requestUri, processResult.Value!.RequestUri);
    }

    [Fact]
    public async Task Validate_InvalidRequestUriFromClient_ExpectInvalidRequestUri()
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
            RequestUri = "invalid_value"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRequestUri, processResult);
    }

    [Fact]
    public async Task Validate_ClientIsUnauthorizedForRequestUri_ExpectUnauthorizedRequestUri()
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
            RequestUri = "https://webapp.authserver.dk/request#4567kebab"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.UnauthorizedRequestUri, processResult);
    }

    [Fact]
    public async Task Validate_RequestUriPointsToInvalidRequestObject_ExpectInvalidRequestObjectFromRequestUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<ISecureRequestService>());
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var requestUri = new RequestUri("https://webapp.authserver.dk/request", client);
        await AddEntity(requestUri);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = $"{requestUri.Uri}#3790kebab"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRequestObjectFromRequestUri, processResult);
    }

    [Fact]
    public async Task Validate_InvalidRequestObject_ExpectInvalidRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<ISecureRequestService>());
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestObject = "invalid_request"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRequest, processResult);
    }

    [Fact]
    public async Task Validate_InvalidState_ExpectInvalidState()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidState, processResult);
    }

    [Fact]
    public async Task Validate_EmptyRedirectUriWithMultipleRegisteredRedirectUris_ExpectInvalidRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var redirectUriOne = new RedirectUri("https://webapp.authserver.dk/callback-one", client);
        var redirectUriTwo = new RedirectUri("https://webapp.authserver.dk/callback-two", client);
        await AddEntity(redirectUriOne);
        await AddEntity(redirectUriTwo);

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16)
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_EmptyRedirectUriWithZeroRegisteredRedirectUris_ExpectInvalidRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClientWithoutRedirectUri();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16)
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_ClientIsUnauthorizedForRedirectUri_ExpectUnauthorizedRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            RedirectUri = "https://webapp.authserver.dk/invalid"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.UnauthorizedRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_InvalidResponseMode_ExpectInvalidResponseMode()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseMode = "invalid_response_mode"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidResponseMode, processResult);
    }

    [Fact]
    public async Task Validate_InvalidResponseType_ExpectInvalidResponseType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = "invalid_response_type"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidResponseType, processResult);
    }

    [Fact]
    public async Task Validate_EmptyResponseType_ExpectInvalidResponseType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16)
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidResponseType, processResult);
    }

    [Fact]
    public async Task Validate_ClientIsUnauthorizedForAuthorizationCode_ExpectUnauthorizedResponseType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClientWithoutGrantType();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.UnauthorizedResponseType, processResult);
    }

    [Fact]
    public async Task Validate_InvalidDisplay_ExpectInvalidDisplay()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Display = "invalid_display"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidDisplay, processResult);
    }

    [Fact]
    public async Task Validate_EmptyNonce_ExpectInvalidNonce()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidNonce, processResult);
    }

    [Fact]
    public async Task Validate_NonceIsNotUnique_ExpectReplayNonce()
    {
        // Arrange
        var nonceRepository = new Mock<INonceRepository>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(nonceRepository);
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var nonce = CryptographyHelper.GetRandomString(16);
        nonceRepository
            .Setup(x => x.IsNonceReplay(nonce, CancellationToken.None))
            .ReturnsAsync(true)
            .Verifiable();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = nonce
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        nonceRepository.Verify();
        Assert.Equal(AuthorizeError.ReplayNonce, processResult);
    }

    [Fact]
    public async Task Validate_InvalidCodeChallengeMethod_ExpectInvalidCodeChallengeMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = "invalid_code_challenge_method"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidCodeChallengeMethod, processResult);
    }

    [Fact]
    public async Task Validate_InvalidCodeChallenge_ExpectInvalidCodeChallenge()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = "invalid_code_challenge"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidCodeChallenge, processResult);
    }

    [Fact]
    public async Task Validate_ScopeDoesNotContainOpenId_ExpectInvalidOpenIdScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidOpenIdScope, processResult);
    }

    [Fact]
    public async Task Validate_ClientIsUnauthorizedForOpenIdScope_ExpectUnauthorizedScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClientWithoutScope();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.UnauthorizedScope, processResult);
    }

    [Fact]
    public async Task Validate_InvalidMaxAge_ExpectInvalidMaxAge()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId],
            MaxAge = "-1"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidMaxAge, processResult);
    }

    [Fact]
    public async Task Validate_InvalidIdTokenHint_ExpectInvalidIdTokenHint()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId],
            IdTokenHint = "invalid_id_token"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidIdTokenHint, processResult);
    }

    [Fact]
    public async Task Validate_InvalidPrompt_ExpectInvalidPrompt()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId],
            Prompt = "invalid_prompt"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidPrompt, processResult);
    }

    [Fact]
    public async Task Validate_InvalidAcrValues_ExpectInvalidAcrValues()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId],
            AcrValues = ["invalid_acr_value"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(AuthorizeError.InvalidAcrValues, processResult);
    }

    [Fact]
    public async Task Validate_RequiresInteraction_ExpectInteraction()
    {
        // Arrange
        var authorizeInteractionService = new Mock<IAuthorizeInteractionService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeInteractionService);
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId]
        };

        authorizeInteractionService
            .Setup(x => x.GetInteractionResult(request, CancellationToken.None))
            .ReturnsAsync(InteractionResult.LoginResult)
            .Verifiable();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        authorizeInteractionService.Verify();
        Assert.Equal(AuthorizeError.LoginRequired, processResult);
    }

    [Fact]
    public async Task Validate_MinimalValidRequest_ExpectAuthorizeValidatedRequest()
    {
        // Arrange
        var authorizeInteractionService = new Mock<IAuthorizeInteractionService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeInteractionService);
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId]
        };

        const string subjectIdentifier = "subjectIdentifier";
        authorizeInteractionService
            .Setup(x => x.GetInteractionResult(request, CancellationToken.None))
            .ReturnsAsync(InteractionResult.Success(subjectIdentifier))
            .Verifiable();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(request.ResponseMode, processResult.Value!.ResponseMode);
        Assert.Equal(request.CodeChallenge, processResult.Value!.CodeChallenge);
        Assert.Equal(request.Scope, processResult.Value!.Scope);
        Assert.Equal(request.AcrValues, processResult.Value!.AcrValues);
        Assert.Equal(request.ClientId, processResult.Value!.ClientId);
        Assert.Equal(request.Nonce, processResult.Value!.Nonce);
        Assert.Equal(request.RedirectUri, processResult.Value!.RedirectUri);
        Assert.Null(processResult.Value!.RequestUri);
    }

    [Fact]
    public async Task Validate_FullValidatedRequest_ExpectAuthorizeValidatedRequest()
    {
        // Arrange
        var authorizeInteractionService = new Mock<IAuthorizeInteractionService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeInteractionService);
        });
        var validator = serviceProvider.GetRequiredService<
            IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>>();

        var client = await GetClient();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            State = CryptographyHelper.GetRandomString(16),
            ResponseType = ResponseTypeConstants.Code,
            Nonce = CryptographyHelper.GetRandomString(16),
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            Scope = [ScopeConstants.OpenId],
            AcrValues = [LevelOfAssuranceSubstantial],
            Display = DisplayConstants.Page,
            MaxAge = "300",
            Prompt = PromptConstants.None,
            RedirectUri = client.RedirectUris.Single().Uri,
            ResponseMode = ResponseModeConstants.FormPost
        };

        const string subjectIdentifier = "subjectIdentifier";
        authorizeInteractionService
            .Setup(x => x.GetInteractionResult(request, CancellationToken.None))
            .ReturnsAsync(InteractionResult.Success(subjectIdentifier))
            .Verifiable();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(request.ResponseMode, processResult.Value!.ResponseMode);
        Assert.Equal(request.CodeChallenge, processResult.Value!.CodeChallenge);
        Assert.Equal(request.Scope, processResult.Value!.Scope);
        Assert.Equal(request.AcrValues, processResult.Value!.AcrValues);
        Assert.Equal(request.ClientId, processResult.Value!.ClientId);
        Assert.Equal(request.Nonce, processResult.Value!.Nonce);
        Assert.Equal(request.RedirectUri, processResult.Value!.RedirectUri);
        Assert.Null(processResult.Value!.RequestUri);
    }

    [Fact]
    public async Task Validate_ValidRequestObjectFromRequestUri_ExpectAuthorizeValidatedRequest()
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

        var client = await GetClient();
        var requestUri = new RequestUri("https://webapp.authserver.dk/request", client);
        await AddEntity(requestUri);

        const string subjectIdentifier = "subjectIdentifier";
        var givenRequestUri = $"{requestUri.Uri}#1234";

        var authorizeRequestDto = new AuthorizeRequestDto
        {
            ResponseMode = ResponseModeConstants.FormPost,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            ResponseType = ResponseTypeConstants.Code,
            Scope = [ScopeConstants.OpenId],
            AcrValues = [LevelOfAssuranceLow],
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(16),
            State = CryptographyHelper.GetRandomString(16),
            RedirectUri = client.RedirectUris.Single().Uri
        };

        secureRequestService
            .Setup(x =>
                x.GetRequestByReference(
                    It.Is<Uri>(y => y.AbsoluteUri == givenRequestUri),
                    client.Id,
                    ClientTokenAudience.AuthorizeEndpoint,
                    CancellationToken.None)
                )
            .ReturnsAsync(authorizeRequestDto)
            .Verifiable();

        authorizeInteractionService
            .Setup(x =>
                x.GetInteractionResult(It.Is<AuthorizeRequest>(y =>
                    y.ResponseMode == authorizeRequestDto.ResponseMode &&
                    y.CodeChallenge == authorizeRequestDto.CodeChallenge &&
                    y.Scope == authorizeRequestDto.Scope &&
                    y.AcrValues == authorizeRequestDto.AcrValues &&
                    y.ClientId == authorizeRequestDto.ClientId &&
                    y.Nonce == authorizeRequestDto.Nonce &&
                    y.RedirectUri == authorizeRequestDto.RedirectUri &&
                    y.CodeChallengeMethod == authorizeRequestDto.CodeChallengeMethod &&
                    y.State == authorizeRequestDto.State &&
                    y.ResponseType == authorizeRequestDto.ResponseType &&
                    y.RequestUri == null), CancellationToken.None))
            .ReturnsAsync(InteractionResult.Success(subjectIdentifier))
            .Verifiable();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestUri = givenRequestUri
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        secureRequestService.Verify();
        authorizeInteractionService.Verify();

        Assert.Equal(subjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(authorizeRequestDto.ResponseMode, processResult.Value!.ResponseMode);
        Assert.Equal(authorizeRequestDto.CodeChallenge, processResult.Value!.CodeChallenge);
        Assert.Equal(authorizeRequestDto.Scope, processResult.Value!.Scope);
        Assert.Equal(authorizeRequestDto.AcrValues, processResult.Value!.AcrValues);
        Assert.Equal(authorizeRequestDto.ClientId, processResult.Value!.ClientId);
        Assert.Equal(authorizeRequestDto.Nonce, processResult.Value!.Nonce);
        Assert.Equal(authorizeRequestDto.RedirectUri, processResult.Value!.RedirectUri);
        Assert.Null(processResult.Value!.RequestUri);
    }

    [Fact]
    public async Task Validate_ValidRequestObjectFromRequest_ExpectAuthorizeValidatedRequest()
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

        var client = await GetClient();

        const string subjectIdentifier = "subjectIdentifier";
        const string givenRequestObject = "request_object";

        var authorizeRequestDto = new AuthorizeRequestDto
        {
            ResponseMode = ResponseModeConstants.FormPost,
            CodeChallenge = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge,
            CodeChallengeMethod = CodeChallengeMethodConstants.S256,
            ResponseType = ResponseTypeConstants.Code,
            Scope = [ScopeConstants.OpenId],
            AcrValues = [LevelOfAssuranceLow],
            ClientId = client.Id,
            Nonce = CryptographyHelper.GetRandomString(16),
            State = CryptographyHelper.GetRandomString(16),
            RedirectUri = client.RedirectUris.Single().Uri
        };

        secureRequestService
            .Setup(x =>
                x.GetRequestByObject(
                    givenRequestObject,
                    client.Id,
                    ClientTokenAudience.AuthorizeEndpoint,
                    CancellationToken.None)
                )
            .ReturnsAsync(authorizeRequestDto)
            .Verifiable();

        authorizeInteractionService
            .Setup(x =>
                x.GetInteractionResult(It.Is<AuthorizeRequest>(y =>
                    y.ResponseMode == authorizeRequestDto.ResponseMode &&
                    y.CodeChallenge == authorizeRequestDto.CodeChallenge &&
                    y.Scope == authorizeRequestDto.Scope &&
                    y.AcrValues == authorizeRequestDto.AcrValues &&
                    y.ClientId == authorizeRequestDto.ClientId &&
                    y.Nonce == authorizeRequestDto.Nonce &&
                    y.RedirectUri == authorizeRequestDto.RedirectUri &&
                    y.CodeChallengeMethod == authorizeRequestDto.CodeChallengeMethod &&
                    y.State == authorizeRequestDto.State &&
                    y.ResponseType == authorizeRequestDto.ResponseType &&
                    y.RequestUri == null), CancellationToken.None))
            .ReturnsAsync(InteractionResult.Success(subjectIdentifier))
            .Verifiable();

        var request = new AuthorizeRequest
        {
            ClientId = client.Id,
            RequestObject = givenRequestObject
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        secureRequestService.Verify();
        authorizeInteractionService.Verify();

        Assert.Equal(subjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(authorizeRequestDto.ResponseMode, processResult.Value!.ResponseMode);
        Assert.Equal(authorizeRequestDto.CodeChallenge, processResult.Value!.CodeChallenge);
        Assert.Equal(authorizeRequestDto.Scope, processResult.Value!.Scope);
        Assert.Equal(authorizeRequestDto.AcrValues, processResult.Value!.AcrValues);
        Assert.Equal(authorizeRequestDto.ClientId, processResult.Value!.ClientId);
        Assert.Equal(authorizeRequestDto.Nonce, processResult.Value!.Nonce);
        Assert.Equal(authorizeRequestDto.RedirectUri, processResult.Value!.RedirectUri);
        Assert.Null(processResult.Value!.RequestUri);
    }

    private async Task<Client> GetClient()
    {
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var redirectUri = new RedirectUri("https://webapp.authserver.dk/callback", client);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);
        var grantType = await GetGrantType(GrantTypeConstants.AuthorizationCode);
        client.GrantTypes.Add(grantType);
        await AddEntity(redirectUri);

        return client;
    }

    private async Task<Client> GetClientWithoutScope()
    {
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var redirectUri = new RedirectUri("https://webapp.authserver.dk/callback", client);
        var grantType = await GetGrantType(GrantTypeConstants.AuthorizationCode);
        client.GrantTypes.Add(grantType);
        await AddEntity(redirectUri);

        return client;
    }

    private async Task<Client> GetClientWithoutGrantType()
    {
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var redirectUri = new RedirectUri("https://webapp.authserver.dk/callback", client);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);
        await AddEntity(redirectUri);

        return client;
    }

    private async Task<Client> GetClientWithoutRedirectUri()
    {
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);
        var grantType = await GetGrantType(GrantTypeConstants.AuthorizationCode);
        client.GrantTypes.Add(grantType);
        await AddEntity(client);

        return client;
    }
}