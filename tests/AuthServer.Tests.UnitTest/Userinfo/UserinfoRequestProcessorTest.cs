using System.Text.Json;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Tests.Core;
using AuthServer.Userinfo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit.Abstractions;
using Claim = System.Security.Claims.Claim;

namespace AuthServer.Tests.UnitTest.Userinfo;
public class UserinfoRequestProcessorTest : BaseUnitTest
{
    public UserinfoRequestProcessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_NoUserinfoSignature_ExpectJsonSerializedClaims()
    {
        // Arrange
        var userClaimService = new Mock<IUserClaimService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(userClaimService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<UserinfoValidatedRequest, string>>();

        var subjectIdentifier = new SubjectIdentifier();

        const string address = "PinguStreet";
        userClaimService
            .Setup(x => x.GetClaims(subjectIdentifier.Id, CancellationToken.None))
            .ReturnsAsync(new[] {new Claim(ClaimNameConstants.Address, address)})
            .Verifiable();

        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false,
            ClientUri = "https://webapp.authserver.dk"
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var jsonClaims = await processor.Process(new UserinfoValidatedRequest
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = [ScopeConstants.OpenId, ScopeConstants.UserInfo, ScopeConstants.Address]
        }, CancellationToken.None);
        var claims = JsonSerializer.Deserialize<IDictionary<string, string>>(jsonClaims)!;

        // Assert
        Assert.Equal(subjectIdentifier.Id, claims[ClaimNameConstants.Sub]);
        Assert.Equal(address, claims[ClaimNameConstants.Address]);
    }

    [Fact]
    public async Task Process_UserinfoSignature_ExpectJwtClaims()
    {
        // Arrange
        var userClaimService = new Mock<IUserClaimService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(userClaimService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<UserinfoValidatedRequest, string>>();

        var subjectIdentifier = new SubjectIdentifier();

        const string address = "PinguStreet";
        userClaimService
            .Setup(x => x.GetClaims(subjectIdentifier.Id, CancellationToken.None))
            .ReturnsAsync(new[] {new Claim(ClaimNameConstants.Address, address)})
            .Verifiable();

        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false,
            ClientUri = "https://webapp.authserver.dk",
            UserinfoSignedResponseAlg = SigningAlg.RsaSha256
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var jsonWebToken = await processor.Process(new UserinfoValidatedRequest
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = [ScopeConstants.OpenId, ScopeConstants.UserInfo, ScopeConstants.Address]
        }, CancellationToken.None);

        // Assert
        Assert.True(TokenHelper.IsJws(jsonWebToken));

        var deserializedToken = new JsonWebTokenHandler().ReadJsonWebToken(jsonWebToken);

        Assert.Equal(subjectIdentifier.Id, deserializedToken.Subject);
        Assert.Equal(address, deserializedToken.GetClaim(ClaimNameConstants.Address).Value);
    }
}
