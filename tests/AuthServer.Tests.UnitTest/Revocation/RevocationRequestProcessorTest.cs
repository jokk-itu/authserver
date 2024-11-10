using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Revocation;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Revocation;
public class RevocationRequestProcessorTest : BaseUnitTest
{
    public RevocationRequestProcessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_ReferenceToken_ExpectRevoked()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var revocationRequestProcessor = serviceProvider.GetRequiredService<IRequestProcessor<RevocationValidatedRequest, Unit>>();
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new ClientAccessToken(client, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        // Act
        await revocationRequestProcessor.Process(new RevocationValidatedRequest
        {
            Token = token.Reference
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task Process_ReferenceTokenAlreadyRevoked_ExpectNoOperation()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var revocationRequestProcessor = serviceProvider.GetRequiredService<IRequestProcessor<RevocationValidatedRequest, Unit>>();
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new ClientAccessToken(client, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        token.Revoke();
        var revokedAt = token.RevokedAt;
        await AddEntity(token);

        // Act
        await revocationRequestProcessor.Process(new RevocationValidatedRequest
        {
            Token = token.Reference
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(token.RevokedAt);
        Assert.Equal(revokedAt, token.RevokedAt);
    }

    [Fact]
    public async Task Process_Jwt_ExpectRevoked()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var revocationRequestProcessor = serviceProvider.GetRequiredService<IRequestProcessor<RevocationValidatedRequest, Unit>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new ClientAccessToken(client, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var tokenHandler = new JsonWebTokenHandler();
        var jwt = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>
            {
                { ClaimNameConstants.Jti, token.Id }
            }
        });

        // Act
        await revocationRequestProcessor.Process(new RevocationValidatedRequest
        {
            Token = jwt
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task Process_InvalidJwt_ExpectNoException()
    {
        // Arrange
        var serverIssuedTokenDecoder = new Mock<ITokenDecoder<ServerIssuedTokenDecodeArguments>>();
        serverIssuedTokenDecoder
            .Setup(x => x.Read(It.IsAny<string>()))
            .ThrowsAsync(new Exception())
            .Verifiable();

        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(serverIssuedTokenDecoder);
        });
        var revocationRequestProcessor = serviceProvider.GetRequiredService<IRequestProcessor<RevocationValidatedRequest, Unit>>();

        // Act
        var exception = await Record.ExceptionAsync(() => revocationRequestProcessor.Process(new RevocationValidatedRequest
        {
            Token = "invalid.jwt.provided"
        }, CancellationToken.None));

        // Assert
        Assert.Null(exception);
        serverIssuedTokenDecoder.Verify();
    }
}
