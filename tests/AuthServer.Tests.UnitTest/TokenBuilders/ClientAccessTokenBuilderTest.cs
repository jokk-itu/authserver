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

public class ClientAccessTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task BuildToken_RequireReferenceToken_ExpectReferenceToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var accessTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<ClientAccessTokenArguments>>();
        var client = await GetClient(true);

        // Act
        var accessToken = await accessTokenBuilder.BuildToken(new ClientAccessTokenArguments
        {
            ClientId = client.Id,
            Scope = [ScopeConstants.OpenId],
            Resource = ["https://localhost:5000"]
        }, CancellationToken.None);
        await IdentityContext.SaveChangesAsync();

        // Assert
        var token = IdentityContext.Set<ClientAccessToken>().Include(x => x.Client).Single();
        Assert.Equal(accessToken, token.Reference);
        Assert.Equal(client.Id, token.Client.Id);
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
        var grantAccessTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<ClientAccessTokenArguments>>();
        var client = await GetClient(false);

        // Act
        var scope = new[] { ScopeConstants.OpenId, ScopeConstants.UserInfo };
        var resource = new[] { "https://localhost:5000", "https://localhost:5001" };
        var accessToken = await grantAccessTokenBuilder.BuildToken(new ClientAccessTokenArguments
        {
            ClientId = client.Id,
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
                ValidAudience = "https://localhost:5000",
                ValidIssuer = DiscoveryDocument.Issuer,
                ValidTypes = [TokenTypeHeaderConstants.AccessToken]
            });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
        Assert.NotNull(validatedTokenResult.Claims[ClaimNameConstants.Jti].ToString());
        Assert.Equal(scope, validatedTokenResult.Claims[ClaimNameConstants.Scope].ToString()!.Split(' '));
        Assert.Equal(client.Id, validatedTokenResult.Claims[ClaimNameConstants.ClientId].ToString());
        Assert.Equal(client.Id, validatedTokenResult.Claims[ClaimNameConstants.Sub].ToString());
        Assert.Equal(resource, validatedTokenResult.Claims[ClaimNameConstants.Aud]);
    }

    private async Task<Client> GetClient(bool requireReferenceToken)
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
        await AddEntity(client);
        return client;
    }
}