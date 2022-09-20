using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using System.Net;
using System.Web;
using Domain.Constants;
using Infrastructure.Helpers;
using WebApp.Constants;
using Xunit;

namespace Specs.Controllers;
public class AuthorizeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _applicationFactory;

	public AuthorizeControllerTests(WebApplicationFactory<Program> applicationFactory)
	{
		_applicationFactory = applicationFactory;
	}

	[Fact]
	[Trait("Category", "Integration")]
	public async Task Authorize_ExpectRedirectResult()
	{
		// Arrange
		var client = _applicationFactory.CreateClient(new WebApplicationFactoryClientOptions 
		{
			 AllowAutoRedirect = false
		});

    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
		var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, "test" },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identity-provider {ScopeConstants.Profile} api1" },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce }
    }.ToQueryString();

    // Act
    var authorizeResponse = await AuthorizeEndpointHelper.GetAuthorizationCodeAsync(client, query, "jokk", "Password12!");
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

		// Assert
		Assert.Equal(HttpStatusCode.Found, authorizeResponse.StatusCode);
		Assert.NotEmpty(queryParameters.Get("code"));
		Assert.Equal(state, queryParameters.Get("state"));
	}
}