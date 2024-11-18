using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System.Net;
using AuthServer.Authorization;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Xunit.Abstractions;
using AuthServer.Authorization.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorization;

public class SecureRequestServiceTest : BaseUnitTest
{
    public SecureRequestServiceTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public void GetCachedRequest_NoPreviousRequest_ExpectInvalidOperationException()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act && Assert
        Assert.Throws<InvalidOperationException>(authorizeRequestParameterService.GetCachedRequest);
    }

    [Fact]
    public async Task GetRequestByObject_InvalidToken_ExpectNull()
    {
        // Arrange
        const string token = "invalid_token";
        const string clientId = "clientId";
        var tokenDecoderMock = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            tokenDecoderMock
                .Setup(x => x.Validate(
                    token,
                    It.Is<ClientIssuedTokenDecodeArguments>(
                        y =>
                            y.ValidateLifetime &&
                            !y.Algorithms.Except(DiscoveryDocument.RequestObjectSigningAlgValuesSupported).Any() &&
                            y.Audience == ClientTokenAudience.AuthorizeEndpoint &&
                            y.ClientId == clientId &&
                            y.TokenType == TokenTypeHeaderConstants.RequestObjectToken),
                    CancellationToken.None))
                .ReturnsAsync((JsonWebToken?)null)
                .Verifiable();

            services.AddScopedMock(tokenDecoderMock);
        });
        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestObject = await authorizeRequestParameterService.GetRequestByObject(token, clientId, ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.Null(requestObject);
        tokenDecoderMock.Verify();
    }

    [Fact]
    public async Task GetRequestObject_ValidToken_ExpectDtoWithValues()
    {
        // Arrange
        var tokenDecoderMock = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(tokenDecoderMock);
        });

        const string value = "value";
        var requestToken = GetRequestObjectToken(value);

        tokenDecoderMock
            .Setup(x => x.Validate(
                requestToken,
                It.Is<ClientIssuedTokenDecodeArguments>(
                    y =>
                        y.ValidateLifetime &&
                        !y.Algorithms.Except(DiscoveryDocument.RequestObjectSigningAlgValuesSupported).Any() &&
                        y.Audience == ClientTokenAudience.AuthorizeEndpoint &&
                        y.ClientId == value &&
                        y.TokenType == TokenTypeHeaderConstants.RequestObjectToken),
                CancellationToken.None))
            .ReturnsAsync(new JsonWebToken(requestToken))
            .Verifiable();

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestObject = await authorizeRequestParameterService.GetRequestByObject(requestToken, value, ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.NotNull(requestObject);
        Assert.Equal(value, requestObject.ClientId);
        Assert.Equal(value, requestObject.CodeChallenge);
        Assert.Equal(value, requestObject.CodeChallengeMethod);
        Assert.Equal(value, requestObject.Display);
        Assert.Equal(value, requestObject.IdTokenHint);
        Assert.Equal(value, requestObject.LoginHint);
        Assert.Equal(value, requestObject.MaxAge);
        Assert.Equal(value, requestObject.Nonce);
        Assert.Equal(value, requestObject.RedirectUri);
        Assert.Equal(value, requestObject.Prompt);
        Assert.Equal(value, requestObject.ResponseMode);
        Assert.Equal(value, requestObject.ResponseType);
        Assert.Equal(value, requestObject.State);
        Assert.Single(requestObject.Scope);
        Assert.Equal(value, requestObject.Scope.Single());
        Assert.Single(requestObject.AcrValues);
        Assert.Equal(value, requestObject.AcrValues.Single());

        tokenDecoderMock.Verify();
    }

    [Fact]
    public async Task GetRequestObject_ValidToken_ExpectDtoWithoutValues()
    {
        // Arrange
        var tokenDecoderMock = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(tokenDecoderMock);
        });

        const string clientId = "clientId";
        var requestToken = GetRequestObjectToken();

        tokenDecoderMock
            .Setup(x => x.Validate(
                requestToken,
                It.Is<ClientIssuedTokenDecodeArguments>(
                    y =>
                        y.ValidateLifetime &&
                        !y.Algorithms.Except(DiscoveryDocument.RequestObjectSigningAlgValuesSupported).Any() &&
                        y.Audience == ClientTokenAudience.AuthorizeEndpoint &&
                        y.ClientId == clientId &&
                        y.TokenType == TokenTypeHeaderConstants.RequestObjectToken),
                CancellationToken.None))
            .ReturnsAsync(new JsonWebToken(requestToken))
            .Verifiable();

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestObject = await authorizeRequestParameterService.GetRequestByObject(requestToken, clientId, ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.NotNull(requestObject);
        Assert.Null(requestObject.ClientId);
        Assert.Null(requestObject.CodeChallenge);
        Assert.Null(requestObject.CodeChallengeMethod);
        Assert.Null(requestObject.Display);
        Assert.Null(requestObject.IdTokenHint);
        Assert.Null(requestObject.LoginHint);
        Assert.Null(requestObject.MaxAge);
        Assert.Null(requestObject.Nonce);
        Assert.Null(requestObject.RedirectUri);
        Assert.Null(requestObject.Prompt);
        Assert.Null(requestObject.ResponseMode);
        Assert.Null(requestObject.ResponseType);
        Assert.Null(requestObject.State);
        Assert.Empty(requestObject.Scope);
        Assert.Empty(requestObject.AcrValues);

        tokenDecoderMock.Verify();
    }

    [Fact]
    public async Task GetRequestByReference_NotOkResponse_ExpectNull()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var requestHandler = new DelegatingHandlerStub("Error", "text/plain", HttpStatusCode.InternalServerError);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });
        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestUri = new Uri($"https://demo.authserver.dk/request-object/{Guid.NewGuid()}");
        var requestObject = await authorizeRequestParameterService.GetRequestByReference(requestUri, "clientId", ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.Null(requestObject);
        httpClientFactory.Verify();
    }

    [Fact]
    public async Task GetRequestByReference_ValidToken_ExpectDtoWithValues()
    {
        // Arrange
        var tokenDecoderMock = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(tokenDecoderMock);
            services.AddSingletonMock(httpClientFactory);
        });

        const string value = "value";
        var requestToken = GetRequestObjectToken(value);

        tokenDecoderMock
            .Setup(x => x.Validate(
                requestToken,
                It.Is<ClientIssuedTokenDecodeArguments>(
                    y =>
                        y.ValidateLifetime &&
                        !y.Algorithms.Except(DiscoveryDocument.RequestObjectSigningAlgValuesSupported).Any() &&
                        y.Audience == ClientTokenAudience.AuthorizeEndpoint &&
                        y.ClientId == value &&
                        y.TokenType == TokenTypeHeaderConstants.RequestObjectToken),
                CancellationToken.None))
            .ReturnsAsync(new JsonWebToken(requestToken))
            .Verifiable();

        var requestHandler = new DelegatingHandlerStub(requestToken, MimeTypeConstants.OAuthRequestJwt, HttpStatusCode.OK);
        httpClientFactory
            .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
            .Returns(new HttpClient(requestHandler))
            .Verifiable();

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestUri = new Uri($"https://demo.authserver.dk/request-object/{Guid.NewGuid()}");
        var requestObject = await authorizeRequestParameterService.GetRequestByReference(requestUri, value, ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.NotNull(requestObject);
        Assert.Equal(value, requestObject.ClientId);
        Assert.Equal(value, requestObject.CodeChallenge);
        Assert.Equal(value, requestObject.CodeChallengeMethod);
        Assert.Equal(value, requestObject.Display);
        Assert.Equal(value, requestObject.IdTokenHint);
        Assert.Equal(value, requestObject.LoginHint);
        Assert.Equal(value, requestObject.MaxAge);
        Assert.Equal(value, requestObject.Nonce);
        Assert.Equal(value, requestObject.RedirectUri);
        Assert.Equal(value, requestObject.Prompt);
        Assert.Equal(value, requestObject.ResponseMode);
        Assert.Equal(value, requestObject.ResponseType);
        Assert.Equal(value, requestObject.State);
        Assert.Single(requestObject.Scope);
        Assert.Equal(value, requestObject.Scope.Single());
        Assert.Single(requestObject.AcrValues);
        Assert.Equal(value, requestObject.AcrValues.Single());

        tokenDecoderMock.Verify();
    }

    [Fact]
    public async Task GetRequestByReference_ValidToken_ExpectDtoWithoutValues()
    {
        // Arrange
        var tokenDecoderMock = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(tokenDecoderMock);
            services.AddSingletonMock(httpClientFactory);
        });

        const string clientId = "clientId";
        var requestToken = GetRequestObjectToken();

        tokenDecoderMock
            .Setup(x => x.Validate(
                requestToken,
                It.Is<ClientIssuedTokenDecodeArguments>(
                    y =>
                        y.ValidateLifetime &&
                        !y.Algorithms.Except(DiscoveryDocument.RequestObjectSigningAlgValuesSupported).Any() &&
                        y.Audience == ClientTokenAudience.AuthorizeEndpoint &&
                        y.ClientId == clientId &&
                        y.TokenType == TokenTypeHeaderConstants.RequestObjectToken),
                CancellationToken.None))
            .ReturnsAsync(new JsonWebToken(requestToken))
            .Verifiable();

        var requestHandler = new DelegatingHandlerStub(requestToken, MimeTypeConstants.OAuthRequestJwt, HttpStatusCode.OK);
        httpClientFactory
            .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
            .Returns(new HttpClient(requestHandler))
            .Verifiable();

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestUri = new Uri($"https://demo.authserver.dk/request-object/{Guid.NewGuid()}");
        var requestObject = await authorizeRequestParameterService.GetRequestByReference(requestUri, clientId, ClientTokenAudience.AuthorizeEndpoint, CancellationToken.None);

        // Assert
        Assert.NotNull(requestObject);
        Assert.Null(requestObject.ClientId);
        Assert.Null(requestObject.CodeChallenge);
        Assert.Null(requestObject.CodeChallengeMethod);
        Assert.Null(requestObject.Display);
        Assert.Null(requestObject.IdTokenHint);
        Assert.Null(requestObject.LoginHint);
        Assert.Null(requestObject.MaxAge);
        Assert.Null(requestObject.Nonce);
        Assert.Null(requestObject.RedirectUri);
        Assert.Null(requestObject.Prompt);
        Assert.Null(requestObject.ResponseMode);
        Assert.Null(requestObject.ResponseType);
        Assert.Null(requestObject.State);
        Assert.Empty(requestObject.Scope);
        Assert.Empty(requestObject.AcrValues);

        tokenDecoderMock.Verify();
    }

    [Fact]
    public async Task GetRequestByPushedRequest_InvalidRequestUri_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();

        var clientId = "clientId";
        var reference = Guid.NewGuid();
        var requestUri = $"{RequestUriConstants.RequestUriPrefix}{reference}";

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestDto = await authorizeRequestParameterService.GetRequestByPushedRequest(requestUri, clientId, CancellationToken.None);

        // Assert
        Assert.Null(requestDto);
    }

    [Fact]
    public async Task GetRequestByPushedRequest_ValidRequestUri_ExpectAuthorizeDto()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientRepository = serviceProvider.GetRequiredService<IClientRepository>();

        var client = new Client("PinguWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequestUriExpiration = 30
        };
        await AddEntity(client);

        var authorizeRequestDto = new AuthorizeRequestDto
        {
            ClientId = client.Id
        };
        var authorizeMessage = await clientRepository.AddAuthorizeMessage(authorizeRequestDto, CancellationToken.None);
        await SaveChangesAsync();

        var requestUri = $"{RequestUriConstants.RequestUriPrefix}{authorizeMessage.Reference}";

        var authorizeRequestParameterService = serviceProvider.GetRequiredService<ISecureRequestService>();

        // Act
        var requestDto = await authorizeRequestParameterService.GetRequestByPushedRequest(requestUri, client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(requestDto);
        Assert.Equal(authorizeRequestDto.ClientId, requestDto.ClientId);
    }

    private string GetRequestObjectToken(string value)
    {
        var jwks = ClientJwkBuilder.GetClientJwks();
        var claims = new Dictionary<string, object>
        {
            { Parameter.ClientId, value },
            { Parameter.CodeChallenge, value },
            { Parameter.CodeChallengeMethod, value },
            { Parameter.Display, value },
            { Parameter.IdTokenHint, value },
            { Parameter.LoginHint, value },
            { Parameter.MaxAge, value },
            { Parameter.Nonce, value },
            { Parameter.RedirectUri, value },
            { Parameter.Prompt, value },
            { Parameter.ResponseMode, value },
            { Parameter.ResponseType, value },
            { Parameter.State, value },
            { Parameter.Scope, value },
            { Parameter.AcrValues, value },
        };
        var requestToken = JwtBuilder.GetRequestObjectJwt(claims, value, jwks.PrivateJwks, ClientTokenAudience.AuthorizeEndpoint);

        return requestToken;
    }

    private string GetRequestObjectToken()
    {
        var jwks = ClientJwkBuilder.GetClientJwks();
        var requestToken = JwtBuilder.GetRequestObjectJwt([], "clientId", jwks.PrivateJwks, ClientTokenAudience.AuthorizeEndpoint);

        return requestToken;
    }
}