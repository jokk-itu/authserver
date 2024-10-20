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

public class RefreshTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task BuildToken_RequireReferenceToken_ExpectReferenceToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
        var authorizationGrant = await GetAuthorizationGrant(true);

        // Act
        var refreshToken = await refreshTokenBuilder.BuildToken(new RefreshTokenArguments
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = [ScopeConstants.OpenId]
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var token = IdentityContext.Set<RefreshToken>().Include(x => x.AuthorizationGrant).Single();
        Assert.Equal(refreshToken, token.Reference);
        Assert.Equal(authorizationGrant.Id, token.AuthorizationGrant.Id);
        Assert.Equal(DiscoveryDocument.Issuer, token.Issuer);
        Assert.Equal(ScopeConstants.OpenId, token.Scope);
        Assert.NotNull(token.ExpiresAt);
        Assert.Equal(authorizationGrant.Client.Id, token.Audience);
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
        var refreshTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
        var authorizationGrant = await GetAuthorizationGrant(false);

        // Act
        var scope = new[] { ScopeConstants.OpenId, ScopeConstants.UserInfo };
        var refreshToken = await refreshTokenBuilder.BuildToken(new RefreshTokenArguments
        {
            AuthorizationGrantId = authorizationGrant.Id,
            Scope = scope
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var token = IdentityContext.Set<RefreshToken>().Include(x => x.AuthorizationGrant).Single();
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(refreshToken, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(signingAlg),
            ValidAudience = authorizationGrant.Client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.RefreshToken],
            NameClaimType = ClaimNameConstants.Name,
            RoleClaimType = ClaimNameConstants.Roles
        });

        Assert.Equal(authorizationGrant.Id, token.AuthorizationGrant.Id);
        Assert.Equal(DiscoveryDocument.Issuer, token.Issuer);
        Assert.Equal(string.Join(' ', scope), token.Scope);
        Assert.NotNull(token.ExpiresAt);
        Assert.Equal(authorizationGrant.Client.Id, token.Audience);

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
        Assert.Equal(authorizationGrant.Session.Id, validatedTokenResult.Claims[ClaimNameConstants.Sid].ToString());
        Assert.Equal(authorizationGrant.Subject, validatedTokenResult.Claims[ClaimNameConstants.Sub].ToString());
        Assert.Equal(token.Id.ToString(), validatedTokenResult.Claims[ClaimNameConstants.Jti].ToString());
        Assert.Equal(authorizationGrant.Id, validatedTokenResult.Claims[ClaimNameConstants.GrantId].ToString());
        Assert.Equal(authorizationGrant.Client.Id, validatedTokenResult.Claims[ClaimNameConstants.ClientId].ToString());
        Assert.Equal(string.Join(' ', scope), validatedTokenResult.Claims[ClaimNameConstants.Scope]);
    }

    private async Task<AuthorizationGrant> GetAuthorizationGrant(bool requireReferenceToken)
    {
        var openIdScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.OpenId);

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireReferenceToken = requireReferenceToken,
            RefreshTokenExpiration = 86400,
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