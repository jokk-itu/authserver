using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.TokenBuilders;

public class GrantAccessTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task BuildToken_RequireReferenceToken_ExpectReferenceToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var accessTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
        var authorizationGrant = await GetAuthorizationGrant(true);

        // Act
        var accessToken = await accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = [ ScopeConstants.OpenId ],
            Resource = ["https://localhost:5000"]
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var token = IdentityContext.Set<GrantAccessToken>().Include(x => x.AuthorizationGrant).Single();
        Assert.Equal(accessToken, token.Reference);
        Assert.Equal(authorizationGrant.Id, token.AuthorizationGrant.Id);
        Assert.Equal(DiscoveryDocument.Issuer, token.Issuer);
        Assert.Equal(ScopeConstants.OpenId, token.Scope);
        Assert.NotNull(token.ExpiresAt);
        Assert.Equal("https://localhost:5000", token.Audience);
    }

    [Theory]
    [InlineData(SigningAlg.RsaSha256)]
    [InlineData(SigningAlg.RsaSha384)]
    [InlineData(SigningAlg.RsaSha512)]
    [InlineData(SigningAlg.RsaSsaPssSha256)]
    [InlineData(SigningAlg.RsaSsaPssSha384)]
    [InlineData(SigningAlg.RsaSsaPssSha512)]
    [InlineData(SigningAlg.EcdsaSha256)]
    [InlineData(SigningAlg.EcdsaSha384)]
    [InlineData(SigningAlg.EcdsaSha512)]
    public async Task BuildToken_StructuredToken_ExpectJwt(SigningAlg signingAlg)
    {
        // Arrange
        TokenSigningAlg = signingAlg;
        var serviceProvider = BuildServiceProvider();
        var grantAccessTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
        var authorizationGrant = await GetAuthorizationGrant(false);

        // Act
        var scope = new[] { ScopeConstants.OpenId, ScopeConstants.UserInfo };
        var resource = new[] { "https://localhost:5000", "https://localhost:5001" };
        var accessToken = await grantAccessTokenBuilder.BuildToken(new GrantAccessTokenArguments
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = scope,
            Resource = resource
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(accessToken,
            new TokenValidationParameters
            {
                IssuerSigningKey = JwksDocument.GetSigningKey(signingAlg),
                ValidAudiences = resource,
                ValidIssuer = DiscoveryDocument.Issuer,
                ValidTypes = [TokenTypeHeaderConstants.AccessToken],
                NameClaimType = ClaimNameConstants.Name,
                RoleClaimType = ClaimNameConstants.Roles
            });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
        Assert.Equal(scope, validatedTokenResult.Claims[ClaimNameConstants.Scope].ToString()!.Split(' '));
        Assert.Equal(authorizationGrant.Session.Id, validatedTokenResult.Claims[ClaimNameConstants.Sid].ToString());
        Assert.Equal(authorizationGrant.Subject, validatedTokenResult.Claims[ClaimNameConstants.Sub].ToString());
        Assert.NotNull(validatedTokenResult.Claims[ClaimNameConstants.Jti].ToString());
        Assert.Equal(authorizationGrant.Id, validatedTokenResult.Claims[ClaimNameConstants.GrantId].ToString());
        Assert.Equal(authorizationGrant.Client.Id, validatedTokenResult.Claims[ClaimNameConstants.ClientId].ToString());
    }

    private async Task<AuthorizationGrant> GetAuthorizationGrant(bool requireReferenceToken)
    {
        var openIdScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.OpenId);

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireReferenceToken = requireReferenceToken,
            AccessTokenExpiration = 300,
            SubjectType = SubjectType.Public
        };

        client.Scopes.Add(openIdScope);

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);

        await AddEntity(authorizationGrant);
        return authorizationGrant;
    }
}