using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorize;

public class AuthorizeRequestParameterServiceTest : BaseUnitTest
{
    public AuthorizeRequestParameterServiceTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task GetCachedRequest_NoPreviousRequest_ExpectInvalidOperationException()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizeRequestParameterService = serviceProvider.GetRequiredService<IAuthorizeRequestParameterService>();

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
                            y.Audience == ClientTokenAudience.TokenEndpoint &&
                            y.ClientId == clientId &&
                            y.TokenTypes.Single() == TokenTypeHeaderConstants.RequestObjectToken),
                    CancellationToken.None))
                .Verifiable();

            services.AddScopedMock(tokenDecoderMock);
        });
        var authorizeRequestParameterService = serviceProvider.GetRequiredService<IAuthorizeRequestParameterService>();

        // Act
        var requestObject = await authorizeRequestParameterService.GetRequestByObject(token, clientId, ClientTokenAudience.TokenEndpoint, CancellationToken.None);

        // Assert
        Assert.Null(requestObject);
        tokenDecoderMock.Verify();
    }
}
