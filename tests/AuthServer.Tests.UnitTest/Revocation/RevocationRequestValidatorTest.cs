using AuthServer.Authentication.Models;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Revocation;
using AuthServer.Revocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Revocation;

public class RevocationRequestValidatorTest : BaseUnitTest
{
    public RevocationRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_InvalidTokenTypeHint_ExpectUnsupportedTokenType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();
        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications = [],
            Token = string.Empty,
            TokenTypeHint = "invalid_token_type_hint"
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.UnsupportedTokenType, processResult);
    }

    [Fact]
    public async Task Validate_NoTokenProvided_ExpectEmptyToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();
        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications = [],
            Token = string.Empty,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.EmptyToken, processResult);
    }

    [Fact]
    public async Task Validate_NoClientAuthentication_ExpectMultipleOrNoneClientMethod()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();
        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications = [],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.MultipleOrNoneClientMethod, processResult);
    }

    [Fact]
    public async Task Validate_InvalidClientAuthenticationMethod_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();
        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications = [new ClientIdAuthentication("client_id")],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_UnauthenticatedClient_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();
        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, "client_id", "client_secret")
            ],
            Token = "token",
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.InvalidClient, processResult);
    }

    [Fact]
    public async Task Validate_MismatchingClientWithReferenceToken_ExpectClientIdDoesNotMatchToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashSecret);
        await AddEntity(client);

        var secondClient = new Client("webapp2", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new ClientAccessToken(secondClient, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainSecret)
            ],
            Token = token.Reference,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.ClientIdDoesNotMatchToken, processResult);
    }

    [Fact]
    public async Task Validate_MismatchingClientWithJwt_ExpectClientIdDoesNotMatchToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashSecret);
        await AddEntity(client);

        var secondClient = new Client("webapp2", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new ClientAccessToken(secondClient, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var key = JwksDocument.GetTokenSigningKey();
        var tokenHandler = new JsonWebTokenHandler();
        var jwt = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>
            {
                { ClaimNameConstants.Jti, token.Id },
                { ClaimNameConstants.ClientId, secondClient.Id },
            },
            SigningCredentials = new SigningCredentials(key.Key, key.Alg.GetDescription()),
            Issuer = token.Issuer,
            IssuedAt = token.IssuedAt,
            Expires = token.ExpiresAt,
            TokenType = TokenTypeHeaderConstants.AccessToken
        });

        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainSecret)
            ],
            Token = jwt,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.Equal(RevocationError.ClientIdDoesNotMatchToken, processResult);
    }

    [Fact]
    public async Task Validate_RevokeReferenceToken_ExpectRevocationValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashSecret);
        var token = new ClientAccessToken(client, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainSecret)
            ],
            Token = token.Reference,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.IsType<RevocationValidatedRequest>(processResult.Value);
        Assert.Equal(token.Reference, processResult.Value.Token);
    }

    [Fact]
    public async Task Validate_RevokeJwt_ExpectRevocationValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<RevocationRequest, RevocationValidatedRequest>>();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashSecret);
        var token = new ClientAccessToken(client, "resource", DiscoveryDocument.Issuer, "scope", DateTime.UtcNow.AddHours(1));
        await AddEntity(token);

        var key = JwksDocument.GetTokenSigningKey();
        var tokenHandler = new JsonWebTokenHandler();
        var jwt = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>
            {
                { ClaimNameConstants.Jti, token.Id },
                { ClaimNameConstants.ClientId, client.Id },
            },
            SigningCredentials = new SigningCredentials(key.Key, key.Alg.GetDescription()),
            Issuer = token.Issuer,
            IssuedAt = token.IssuedAt,
            Expires = token.ExpiresAt,
            TokenType = TokenTypeHeaderConstants.AccessToken
        });

        var revocationRequest = new RevocationRequest
        {
            ClientAuthentications =
            [
                new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainSecret)
            ],
            Token = jwt,
            TokenTypeHint = TokenTypeConstants.AccessToken
        };

        // Act
        var processResult = await validator.Validate(revocationRequest, CancellationToken.None);

        // Assert
        Assert.IsType<RevocationValidatedRequest>(processResult.Value);
        Assert.Equal(jwt, processResult.Value.Token);
    }
}