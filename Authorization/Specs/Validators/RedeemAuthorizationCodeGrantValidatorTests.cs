using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.RedeemAuthorizationCodeGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class RedeemAuthorizationCodeGrantValidatorTests : BaseUnitTest
{
    [Fact]
    public async Task ValidateAsync_ExpectInvalidCodeVerifier()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] {ScopeConstants.OpenId});

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = "123"
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectInvalidScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = "invalid_scope"
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidScope, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectInvalidGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = string.Empty
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectInvalidGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = GrantTypeConstants.AuthorizationCode
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidGrant, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_InvalidClientId_ExpectInvalidClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          authorizationCodeGrant.Id,
          authorizationCodeGrant.AuthorizationCodes.Single().Id,
          authorizationCodeGrant.Nonces.Single().Id,
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = GrantTypeConstants.AuthorizationCode,
            ClientId = string.Empty,
            ClientSecret = authorizationCodeGrant.Client.Secret
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_InvalidClientSecret_ExpectInvalidClient()
    {
      // Arrange
      var serviceProvider = BuildServiceProvider();
      var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
      var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
      var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
      var code = await codeBuilder.BuildAuthorizationCodeAsync(
        authorizationCodeGrant.Id,
        authorizationCodeGrant.AuthorizationCodes.Single().Id,
        authorizationCodeGrant.Nonces.Single().Id,
        pkce.CodeChallenge,
        CodeChallengeMethodConstants.S256,
        new[] { ScopeConstants.OpenId });

      var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
      var command = new RedeemAuthorizationCodeGrantCommand
      {
        Code = code,
        CodeVerifier = pkce.CodeVerifier,
        Scope = $"{ScopeConstants.OpenId}",
        GrantType = GrantTypeConstants.AuthorizationCode,
        ClientId = authorizationCodeGrant.Client.Id,
        ClientSecret = string.Empty
      };

      // Act
      var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

      // Assert
      Assert.True(validationResult.IsError());
      Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectUnauthorizedClient()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          authorizationCodeGrant.Id,
          authorizationCodeGrant.AuthorizationCodes.Single().Id,
          authorizationCodeGrant.Nonces.Single().Id,
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = GrantTypeConstants.AuthorizationCode,
            ClientId = authorizationCodeGrant.Client.Id,
            ClientSecret = authorizationCodeGrant.Client.Secret
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectInvalidSession()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
        authorizationCodeGrant.Session.IsRevoked = true;
        await IdentityContext.SaveChangesAsync();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          authorizationCodeGrant.Id,
          authorizationCodeGrant.AuthorizationCodes.Single().Id,
          authorizationCodeGrant.Nonces.Single().Id,
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = GrantTypeConstants.AuthorizationCode,
            ClientId = authorizationCodeGrant.Client.Id,
            ClientSecret = authorizationCodeGrant.Client.Secret,
            RedirectUri = "https://localhost:5001/callback"
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidGrant, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_NullConsentGrant_ConsentRequired()
    {
      // Arrange
      var serviceProvider = BuildServiceProvider();
      var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
      authorizationCodeGrant.Client.ConsentGrants.Clear();
      authorizationCodeGrant.Session.User.ConsentGrants.Clear();
      await IdentityContext.SaveChangesAsync();
      var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
      var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
      var code = await codeBuilder.BuildAuthorizationCodeAsync(
        authorizationCodeGrant.Id,
        authorizationCodeGrant.AuthorizationCodes.Single().Id,
        authorizationCodeGrant.Nonces.Single().Id,
        pkce.CodeChallenge,
        CodeChallengeMethodConstants.S256,
        new[] { ScopeConstants.OpenId });

      var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
      var command = new RedeemAuthorizationCodeGrantCommand
      {
        Code = code,
        CodeVerifier = pkce.CodeVerifier,
        Scope = $"{ScopeConstants.OpenId}",
        GrantType = GrantTypeConstants.AuthorizationCode,
        ClientId = authorizationCodeGrant.Client.Id,
        ClientSecret = authorizationCodeGrant.Client.Secret,
        RedirectUri = "https://localhost:5001/callback"
      };

      // Act
      var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

      // Assert
      Assert.True(validationResult.IsError());
      Assert.Equal(ErrorCode.ConsentRequired, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ScopeExceedsRequestedScope_InvalidScope()
    {
      // Arrange
      var serviceProvider = BuildServiceProvider();
      var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new List<string>());
      await IdentityContext.SaveChangesAsync();
      var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
      var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
      var code = await codeBuilder.BuildAuthorizationCodeAsync(
        authorizationCodeGrant.Id,
        authorizationCodeGrant.AuthorizationCodes.Single().Id,
        authorizationCodeGrant.Nonces.Single().Id,
        pkce.CodeChallenge,
        CodeChallengeMethodConstants.S256,
        new[] { ScopeConstants.OpenId });

      var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
      var command = new RedeemAuthorizationCodeGrantCommand
      {
        Code = code,
        CodeVerifier = pkce.CodeVerifier,
        Scope = $"{ScopeConstants.OpenId}",
        GrantType = GrantTypeConstants.AuthorizationCode,
        ClientId = authorizationCodeGrant.Client.Id,
        ClientSecret = authorizationCodeGrant.Client.Secret,
        RedirectUri = "https://localhost:5001/callback"
      };

      // Act
      var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

      // Assert
      Assert.True(validationResult.IsError());
      Assert.Equal(ErrorCode.InvalidScope, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_ExpectOk()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationCodeGrant = await GetAuthorizationGrant(ApplicationType.Web, new[] {ScopeConstants.OpenId});
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync(
          authorizationCodeGrant.Id,
          authorizationCodeGrant.AuthorizationCodes.Single().Id,
          authorizationCodeGrant.Nonces.Single().Id,
          pkce.CodeChallenge,
          CodeChallengeMethodConstants.S256,
          new[] { ScopeConstants.OpenId });

        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = pkce.CodeVerifier,
            Scope = $"{ScopeConstants.OpenId}",
            GrantType = GrantTypeConstants.AuthorizationCode,
            ClientId = authorizationCodeGrant.Client.Id,
            ClientSecret = authorizationCodeGrant.Client.Secret,
            RedirectUri = "https://localhost:5001/callback"
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.False(validationResult.IsError());
    }

    private async Task<AuthorizationCodeGrant> GetAuthorizationGrant(ApplicationType applicationType, ICollection<string> scopes)
    {
      var consentedScopes = await IdentityContext
        .Set<Scope>()
        .Where(x => scopes.Any(y => y == x.Name))
        .ToListAsync();

      var consentGrant = ConsentGrantBuilder
        .Instance()
        .AddScopes(consentedScopes)
        .Build();

      var grantType = await IdentityContext
            .Set<GrantType>()
            .SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);

        var client = ClientBuilder
            .Instance()
            .AddGrantType(grantType)
            .AddRedirectUri("https://localhost:5001/callback")
            .AddConsentGrant(consentGrant)
            .AddApplicationType(applicationType)
            .Build();

        var nonce = NonceBuilder
          .Instance(Guid.NewGuid().ToString())
          .Build();

        var authorizationCode = AuthorizationCodeBuilder
          .Instance(Guid.NewGuid().ToString())
          .Build();

        var authorizationCodeGrant = AuthorizationCodeGrantBuilder
            .Instance(Guid.NewGuid().ToString())
            .AddAuthorizationCode(authorizationCode)
            .AddNonce(nonce)
            .AddClient(client)
            .Build();

        var session = SessionBuilder
            .Instance()
            .AddAuthorizationCodeGrant(authorizationCodeGrant)
            .Build();

        var user = UserBuilder
            .Instance()
            .AddPassword(CryptographyHelper.GetRandomString(16))
            .AddSession(session)
            .AddConsentGrant(consentGrant)
            .Build();

        await IdentityContext.AddAsync(user);
        await IdentityContext.SaveChangesAsync();

        return authorizationCodeGrant;
    }
}