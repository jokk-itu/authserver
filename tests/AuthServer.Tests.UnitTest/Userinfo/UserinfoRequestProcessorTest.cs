using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Userinfo;
using Microsoft.Extensions.DependencyInjection;
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

        var subjectIdentifier = new PublicSubjectIdentifier();

        const string address = "PinguStreet";
        userClaimService
            .Setup(x => x.GetClaims(subjectIdentifier.Id, CancellationToken.None))
            .ReturnsAsync(new[] {new Claim(ClaimNameConstants.Address, address)})
            .Verifiable();

        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var grant = new AuthorizationGrant(DateTime.UtcNow, session, client, subjectIdentifier);
        await AddEntity(grant);

        // Act
        var jsonClaims = await processor.Process(new UserinfoValidatedRequest
        {
            AuthorizationGrantId = grant.Id,
            Scope = [ScopeConstants.OpenId, ScopeConstants.UserInfo, ScopeConstants.Address]
        }, CancellationToken.None);
        var claims = JsonSerializer.Deserialize<IDictionary<string, string>>(jsonClaims)!;

        // Assert
        Assert.Equal(subjectIdentifier.Id, claims[ClaimNameConstants.Sub]);
        Assert.Equal(address, claims[ClaimNameConstants.Address]);
    }
}
