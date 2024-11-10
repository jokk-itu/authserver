using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Tests.Core;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.TokenBuilders;

public class LogoutTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
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
    public async Task BuildToken_NoRegisteredEncryption_IsOnlySignedToken(SigningAlg signingAlg)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IClientJwkService>());
        });
        var logoutTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<LogoutTokenArguments>>();
        var authorizationGrant = await GetAuthorizationGrant(signingAlg);

        // Act
        var token = await logoutTokenBuilder.BuildToken(new LogoutTokenArguments
        {
            ClientId = authorizationGrant.Client.Id,
            SessionId = authorizationGrant.Session.Id,
            SubjectIdentifier = authorizationGrant.Subject
        }, CancellationToken.None);

        // Assert
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(signingAlg),
            ValidAudience = authorizationGrant.Client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.LogoutToken],
            NameClaimType = ClaimNameConstants.Name,
            RoleClaimType = ClaimNameConstants.Roles
        });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
        Assert.Equal(authorizationGrant.Session.Id, validatedTokenResult.Claims[ClaimNameConstants.Sid].ToString());
        Assert.Equal(authorizationGrant.Subject, validatedTokenResult.Claims[ClaimNameConstants.Sub].ToString());
        Assert.Equal(authorizationGrant.Client.Id, validatedTokenResult.Claims[ClaimNameConstants.ClientId].ToString());
        Assert.NotNull(validatedTokenResult.Claims[ClaimNameConstants.Jti]);
        Assert.NotNull(validatedTokenResult.Claims[ClaimNameConstants.Events]);
    }

    [Theory]
    [InlineData(EncryptionAlg.RsaPKCS1, EncryptionEnc.Aes128CbcHmacSha256)]
    [InlineData(EncryptionAlg.RsaPKCS1, EncryptionEnc.Aes192CbcHmacSha384)]
    [InlineData(EncryptionAlg.RsaPKCS1, EncryptionEnc.Aes256CbcHmacSha512)]
    [InlineData(EncryptionAlg.RsaOAEP, EncryptionEnc.Aes128CbcHmacSha256)]
    [InlineData(EncryptionAlg.RsaOAEP, EncryptionEnc.Aes192CbcHmacSha384)]
    [InlineData(EncryptionAlg.RsaOAEP, EncryptionEnc.Aes256CbcHmacSha512)]
    public async Task BuildToken_RsaRegisteredEncryption_IsEncryptedAndSignedToken(EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var logoutTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<LogoutTokenArguments>>();
        var clientJwk = ClientJwkBuilder.GetClientJwks(SigningAlg.RsaSha256, encryptionAlg);
        var authorizationGrant = await GetAuthorizationGrant(SigningAlg.RsaSha256, encryptionAlg, encryptionEnc, clientJwk.PublicJwks);

        // Act
        var token = await logoutTokenBuilder.BuildToken(new LogoutTokenArguments
        {
            ClientId = authorizationGrant.Client.Id,
            SessionId = authorizationGrant.Session.Id,
            SubjectIdentifier = authorizationGrant.Subject
        }, CancellationToken.None);

        // Assert
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(SigningAlg.RsaSha256),
            TokenDecryptionKeys = JsonWebKeySet.Create(clientJwk.PrivateJwks).Keys,
            ValidAudience = authorizationGrant.Client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.LogoutToken]
        });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
    }

    [Theory]
    [InlineData(EncryptionAlg.EcdhEsA128KW, EncryptionEnc.Aes128CbcHmacSha256)]
    [InlineData(EncryptionAlg.EcdhEsA192KW, EncryptionEnc.Aes192CbcHmacSha384)]
    [InlineData(EncryptionAlg.EcdhEsA256KW, EncryptionEnc.Aes256CbcHmacSha512)]
    public async Task BuildToken_EcdhRegisteredEncryption_IsEncryptedAndSignedToken(EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var logoutTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<LogoutTokenArguments>>();
        var clientJwk = ClientJwkBuilder.GetClientJwks(SigningAlg.RsaSha256, encryptionAlg);
        var authorizationGrant = await GetAuthorizationGrant(SigningAlg.RsaSha256, encryptionAlg, encryptionEnc, clientJwk.PublicJwks);

        // Act
        var token = await logoutTokenBuilder.BuildToken(new LogoutTokenArguments
        {
            ClientId = authorizationGrant.Client.Id,
            SessionId = authorizationGrant.Session.Id,
            SubjectIdentifier = authorizationGrant.Subject
        }, CancellationToken.None);

        // Assert
        var clientJsonWebKey = JsonWebKeySet.Create(clientJwk.PrivateJwks).Keys
            .Single(x => x.Alg == encryptionAlg.GetDescription());
        var clientPrivateKey = TokenHelper.ConvertToSecurityKey(clientJsonWebKey, true);
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(SigningAlg.RsaSha256),
            TokenDecryptionKeyResolver = (_, _, _, _) => [clientPrivateKey],
            TokenDecryptionKey = JwksDocument.GetEncryptionKey(encryptionAlg),
            ValidAudience = authorizationGrant.Client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.LogoutToken]
        });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
    }

    private async Task<AuthorizationGrant> GetAuthorizationGrant(SigningAlg signingAlg)
    {
        var openIdScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.OpenId);

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            IdTokenSignedResponseAlg = signingAlg,
            SubjectType = SubjectType.Pairwise
        };

        client.Scopes.Add(openIdScope);

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant( session, client, subjectIdentifier.Id, lowAcr);

        await AddEntity(authorizationGrant);
        return authorizationGrant;
    }

    private async Task<AuthorizationGrant> GetAuthorizationGrant(SigningAlg signingAlg, EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc, string clientJwks)
    {
        var openIdScope = await IdentityContext
            .Set<Scope>()
            .SingleAsync(x => x.Name == ScopeConstants.OpenId);

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            IdTokenSignedResponseAlg = signingAlg,
            IdTokenEncryptedResponseAlg = encryptionAlg,
            IdTokenEncryptedResponseEnc = encryptionEnc,
            SubjectType = SubjectType.Pairwise,
            Jwks = clientJwks
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