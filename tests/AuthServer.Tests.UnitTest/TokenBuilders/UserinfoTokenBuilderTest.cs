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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.TokenBuilders;

public class UserinfoTokenBuilderTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
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
        var userinfoTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<UserinfoTokenArguments>>();
        var clientId = Guid.NewGuid().ToString();

        // Act
        var token = await userinfoTokenBuilder.BuildToken(new UserinfoTokenArguments
        {
            ClientId = clientId,
            SigningAlg = signingAlg,
            EndUserClaims = new Dictionary<string, object>
            {
                { ClaimNameConstants.Name, "Pingu" }
            }.AsReadOnly()
        }, CancellationToken.None);

        // Assert
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(signingAlg),
            ValidAudience = clientId,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [ TokenTypeHeaderConstants.UserinfoToken ],
            NameClaimType = ClaimNameConstants.Name,
            RoleClaimType = ClaimNameConstants.Roles
        });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
        Assert.Equal("Pingu", validatedTokenResult.ClaimsIdentity.Name);
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
        var userinfoTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<UserinfoTokenArguments>>();
        var clientJwk = ClientJwkBuilder.GetClientJwks(SigningAlg.RsaSha256, encryptionAlg);
        var client = new Client("PinguPrivateKeyJwtWebApp", ApplicationType.Web, TokenEndpointAuthMethod.PrivateKeyJwt)
        {
            Jwks = clientJwk.PublicJwks
        };
        await AddEntity(client);

        // Act
        var token = await userinfoTokenBuilder.BuildToken(new UserinfoTokenArguments
        {
            ClientId = client.Id,
            SigningAlg = SigningAlg.RsaSha256,
            EndUserClaims = new Dictionary<string, object>().AsReadOnly(),
            EncryptionAlg = encryptionAlg,
            EncryptionEnc = encryptionEnc
        }, CancellationToken.None);

        // Assert
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(SigningAlg.RsaSha256),
            TokenDecryptionKeys = JsonWebKeySet.Create(clientJwk.PrivateJwks).Keys,
            ValidAudience = client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.UserinfoToken]
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
        var userinfoTokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<UserinfoTokenArguments>>();
        var clientJwk = ClientJwkBuilder.GetClientJwks(SigningAlg.RsaSha256, encryptionAlg);
        var client = new Client("PinguPrivateKeyJwtWebApp", ApplicationType.Web, TokenEndpointAuthMethod.PrivateKeyJwt)
        {
            Jwks = clientJwk.PublicJwks
        };
        await AddEntity(client);

        // Act
        var token = await userinfoTokenBuilder.BuildToken(new UserinfoTokenArguments
        {
            ClientId = client.Id,
            SigningAlg = SigningAlg.RsaSha256,
            EndUserClaims = new Dictionary<string, object>().AsReadOnly(),
            EncryptionAlg = encryptionAlg,
            EncryptionEnc = encryptionEnc
        }, CancellationToken.None);

        // Assert
        var clientJsonWebKey = JsonWebKeySet.Create(clientJwk.PrivateJwks).Keys
            .Single(x => x.Alg == encryptionAlg.GetDescription());
        var clientPrivateKey = TokenHelper.ConvertToSecurityKey(clientJsonWebKey, true);
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var validatedTokenResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = JwksDocument.GetSigningKey(SigningAlg.RsaSha256),
            TokenDecryptionKeyResolver = (_, _, _, _) => [ clientPrivateKey ],
            TokenDecryptionKey = JwksDocument.GetEncryptionKey(encryptionAlg),
            ValidAudience = client.Id,
            ValidIssuer = DiscoveryDocument.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.UserinfoToken]
        });

        Assert.NotNull(validatedTokenResult);
        Assert.Null(validatedTokenResult.Exception);
        Assert.True(validatedTokenResult.IsValid);
    }
}