using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Specs.Helpers;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
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

    var state = RandomGeneratorHelper.GeneratorRandomString(16);
		var nonce = RandomGeneratorHelper.GeneratorRandomString(32);
		var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { "response_type", "code" },
      { "client_id", "test" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "state", state },
      { "code_challenge", pkce.CodeChallenge },
      { "code_challenge_method", "S256" },
      { "nonce", nonce }
    }.ToQueryString();

    // Act
    var loginViewResponse = await client.GetAsync($"connect/v1/authorize{query}");
		var (cookie, field) = await AntiForgeryHelper.GetAntiForgeryAsync(loginViewResponse);

		var postAuthorizeRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/v1/authorize{query}");
		postAuthorizeRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", cookie).ToString());
		var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "username", "jokk" },
			{ "password", "Password12!" },
			{ "AntiForgeryField", field }
		});
		postAuthorizeRequest.Content = loginForm;
		var authorizeResponse = await client.SendAsync(postAuthorizeRequest);
		var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

		// Assert
		Assert.Equal(HttpStatusCode.Found, authorizeResponse.StatusCode);
		Assert.NotEmpty(queryParameters.Get("code"));
		Assert.Equal(state, queryParameters.Get("state"));
	}
}