using System.Net;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Enums;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class AuthorizeIntegrationTest : BaseIntegrationTest
{
    public AuthorizeIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Authorize_NoPromptWithRequestObject_ExpectRedirectCode()
    {
        // Arrange
        var jwks = ClientJwkBuilder.GetClientJwks();
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .WithJwks(jwks.PublicJwks)
            .WithRequestObjectSigningAlg(SigningAlg.RsaSha256)
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithRequest(jwks.PrivateJwks)
            .WithAuthorizeUser()
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponse.LocationUri);
        Assert.NotNull(authorizeResponse.Code);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectCode()
    {
        // Arrange
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithAuthorizeUser()
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponse.LocationUri);
        Assert.NotNull(authorizeResponse.Code);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectLogin()
    {
        // Arrange
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithAuthorizeUser()
            .WithMaxAge(0)
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(UserInteraction.LoginUri, authorizeResponse.LocationUri);
        Assert.Equal(authorizeResponse.RequestUri, authorizeResponse.ReturnUrl);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectConsent()
    {
        // Arrange
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithAuthorizeUser()
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(UserInteraction.ConsentUri, authorizeResponse.LocationUri);
        Assert.Equal(authorizeResponse.RequestUri, authorizeResponse.ReturnUrl);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectSelectAccount()
    {
        // Arrange
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(UserInteraction.AccountSelectionUri, authorizeResponse.LocationUri);
        Assert.Equal(authorizeResponse.RequestUri, authorizeResponse.ReturnUrl);
    }

    [Fact]
    public async Task Authorize_InvalidClientId_ExpectBadRequest()
    {
        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId("invalid_client_id")
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, authorizeResponse.StatusCode);
        Assert.Equal(ErrorCode.InvalidClient, authorizeResponse.Error);
        Assert.NotNull(authorizeResponse.ErrorDescription);
    }

    [Fact]
    public async Task Authorize_InvalidScope_ExpectRedirectInvalidScope()
    {
        // Arrange
        var registerResponse = await RegisterEndpointBuilder
            .WithRedirectUris(["https://webapp.authserver.dk/"])
            .WithClientName("webapp")
            .Post();

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponse = await AuthorizeEndpointBuilder
            .WithClientId(registerResponse.ClientId)
            .WithScope(["invalid_scope"])
            .WithAuthorizeUser()
            .Get();

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponse.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponse.LocationUri);
        Assert.Equal(ErrorCode.InvalidScope, authorizeResponse.Error);
        Assert.NotNull(authorizeResponse.ErrorDescription);
    }
}
