using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.RequestAccessors.Userinfo;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using AuthServer.Userinfo;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Userinfo;
public class UserinfoRequestValidatorTest : BaseUnitTest
{
    public UserinfoRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_Jwt_ExpectUserinfoValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>>();
        var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var scope = new[] { ScopeConstants.OpenId, ScopeConstants.UserInfo };
        var resource = new[] { client.ClientUri } ;
        var jwt = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = scope,
            Resource = resource
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        var request = new UserinfoRequest
        {
            AccessToken = jwt
        };

        // Act
        var userinfoValidatedRequest = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.IsType<UserinfoValidatedRequest>(userinfoValidatedRequest.Value);
        Assert.Equal(authorizationGrant.Id, userinfoValidatedRequest.Value.AuthorizationGrantId);
        Assert.Equal(scope, userinfoValidatedRequest.Value.Scope);
    }

    [Fact]
    public async Task Validate_ReferenceToken_ExpectUserinfoValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://webapp.authserver.dk"
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        var grantAccessToken = new GrantAccessToken(authorizationGrant, client.ClientUri, DiscoveryDocument.Issuer,
            $"{ScopeConstants.OpenId} {ScopeConstants.UserInfo}", DateTime.UtcNow.AddHours(1));

        await AddEntity(grantAccessToken);

        var request = new UserinfoRequest
        {
            AccessToken = grantAccessToken.Reference
        };

        // Act
        var userinfoValidatedRequest = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.IsType<UserinfoValidatedRequest>(userinfoValidatedRequest.Value);
        Assert.Equal(authorizationGrant.Id, userinfoValidatedRequest.Value.AuthorizationGrantId);
        Assert.Equal(grantAccessToken.Scope!.Split(' '), userinfoValidatedRequest.Value.Scope);
    }
}
