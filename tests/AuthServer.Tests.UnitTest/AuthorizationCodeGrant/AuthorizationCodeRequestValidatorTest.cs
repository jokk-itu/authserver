using AuthServer.Authentication.Models;
using AuthServer.Codes;
using AuthServer.Codes.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Token;
using AuthServer.Tests.Core;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.AuthorizationCodeGrant;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;
using ProofKeyForCodeExchangeHelper = AuthServer.Tests.Core.ProofKeyForCodeExchangeHelper;

namespace AuthServer.Tests.UnitTest.AuthorizationCodeGrant;

public class AuthorizationCodeRequestValidatorTest : BaseUnitTest
{
    public AuthorizationCodeRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_EmptyGrantType_ExpectUnsupportedGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var request = new TokenRequest();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnsupportedGrantType, processResult);
    }

    [Fact]
    public async Task Validate_EmptyResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
    }

    [Fact]
    public async Task Validate_NullCode_ExpectInvalidCode()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidCode, processResult);
    }

    [Fact]
    public async Task Validate_NullCodeVerifier_ExpectInvalidCodeVerifier()
    {
        // Arrange
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = string.Empty,
                AuthorizationGrantId = string.Empty,
                CodeChallenge = string.Empty,
                Scope = []
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidCodeVerifier, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_MismatchingRedirectUris_ExpectInvalidRedirectUri()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = string.Empty,
                AuthorizationGrantId = string.Empty,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
                RedirectUri = "valid_redirect_uri"
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            RedirectUri = "invalid_redirect_uri",
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidRedirectUri, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_NoClientAuthentication_ExpectMultipleOrNoneClientMethod()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = string.Empty,
                AuthorizationGrantId = string.Empty,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.MultipleOrNoneClientMethod, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_InvalidClientAuthentication_ExpectInvalidClient()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = string.Empty,
                AuthorizationGrantId = string.Empty,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientIdAuthentication("clientId")
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidClient, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_RevokedGrant_ExpectInvalidGrant()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        authorizationGrant.Revoke();
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;
        await SaveChangesAsync();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidGrant, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_RedeemedAuthorizationCode_ExpectInvalidGrant()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCode = authorizationGrant.AuthorizationCodes.Single();
        authorizationCode.Redeem();
        await SaveChangesAsync();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCode.Id,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidGrant, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_ExpiredAuthorizationCode_ExpectInvalidGrant()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCode = authorizationGrant.AuthorizationCodes.Single();
        typeof(AuthorizationCode)
            .GetProperty(nameof(AuthorizationCode.ExpiresAt))!
            .SetValue(authorizationCode, DateTime.UtcNow.AddSeconds(-60));

        await SaveChangesAsync();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCode.Id,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidGrant, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_UnauthorizedForAuthorizationCode_ExpectUnauthorizedForGrantType()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        authorizationGrant.Client.GrantTypes.Clear();
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;
        await SaveChangesAsync();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForGrantType, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_UnauthorizedRedirectUri_ExpectUnauthorizedForRedirectUri()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            RedirectUri = "https://client.authserver.dk/invalid-callback",
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForRedirectUri, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_UnauthorizedScopeForClient_ExpectUnauthorizedForScope()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [ScopeConstants.Profile],
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnauthorizedForScope, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_NoConsentedScope_ExpectConsentRequired()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        authorizationGrant.Client.ConsentGrants.Single().ConsentedScopes.Clear();
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;
        await SaveChangesAsync();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [ScopeConstants.OpenId]
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.ConsentRequired, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_ExceedConsentedScope_ExpectScopeExceedsConsentedScope()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [ScopeConstants.OpenId, ScopeConstants.Profile]
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.ScopeExceedsConsentedScope, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_ResourceDoesNotExist_ExpectInvalidTarget()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                Scope = [ScopeConstants.OpenId]
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = ["resource"],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.InvalidTarget, processResult);
        authorizationCodeEncoder.Verify();
    }

    [Fact]
    public async Task Validate_ValidatedRequest_ExpectValidatedRequest()
    {
        // Arrange
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var authorizationCodeEncoder = new Mock<IAuthorizationCodeEncoder>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizationCodeEncoder);
        });

        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>>();

        var plainSecret = CryptographyHelper.GetRandomString(32);
        var authorizationGrant = await GetAuthorizationGrant(plainSecret);
        var redirectUri = authorizationGrant.Client.RedirectUris.Single().Uri;
        var authorizationCodeId = authorizationGrant.AuthorizationCodes.Single().Id;
        var weatherClient = await GetWeatherClient();

        authorizationCodeEncoder
            .Setup(x => x.DecodeAuthorizationCode(It.IsAny<string>()))
            .Returns(new EncodedAuthorizationCode
            {
                AuthorizationCodeId = authorizationCodeId,
                AuthorizationGrantId = authorizationGrant.Id,
                CodeChallenge = proofKeyForCodeExchange.CodeChallenge,
                RedirectUri = redirectUri,
                Scope = [ScopeConstants.OpenId]
            })
            .Verifiable();

        var request = new TokenRequest
        {
            GrantType = GrantTypeConstants.AuthorizationCode,
            Resource = [weatherClient.ClientUri!],
            CodeVerifier = proofKeyForCodeExchange.CodeVerifier,
            RedirectUri = redirectUri,
            ClientAuthentications =
            [
                new ClientSecretAuthentication(
                    TokenEndpointAuthMethod.ClientSecretBasic,
                    authorizationGrant.Client.Id,
                    plainSecret)
            ]
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(authorizationGrant.Id, processResult.Value!.AuthorizationGrantId);
        Assert.Equal(authorizationCodeId, processResult.Value!.AuthorizationCodeId);
        Assert.Equal(request.Resource, processResult.Value!.Resource);
        Assert.Equal([ScopeConstants.OpenId], processResult.Value!.Scope);
        authorizationCodeEncoder.Verify();
    }

    private async Task<AuthorizationGrant> GetAuthorizationGrant(string plainSecret)
    {
        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);

        var redirectUri = new RedirectUri("https://webapp.authserver.dk/callback", client);

        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);

        var authorizationCodeGrantType = await GetGrantType(GrantTypeConstants.AuthorizationCode);
        client.GrantTypes.Add(authorizationCodeGrantType);

        var consentGrant = new ConsentGrant(subjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(openIdScope);

        var authenticationContextReference = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, authenticationContextReference);
        var authorizationCode = new AuthorizationCode(authorizationGrant, 60);
        authorizationCode.SetValue("authorization_code");

        await AddEntity(redirectUri);
        await AddEntity(authorizationCode);
        await AddEntity(consentGrant);

        return authorizationGrant;
    }

    private async Task<Client> GetWeatherClient()
    {
        var weatherClient = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://weather.authserver.dk"
        };
        var openIdScope = await GetScope(ScopeConstants.OpenId);
        weatherClient.Scopes.Add(openIdScope);
        await AddEntity(weatherClient);
        return weatherClient;
    }
}
