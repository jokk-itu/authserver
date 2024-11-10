using System.Text;
using AuthServer.Authentication.Models;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class TokenRequestAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
	[Theory]
	[InlineData("", "")]
	[InlineData(null, null)]
	public async Task GetRequest_EmptyStringParameters_ExpectNoValues(string? value, string? expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.GrantType, value },
			{ Parameter.Code, value },
			{ Parameter.CodeVerifier, value },
			{ Parameter.RedirectUri, value },
			{ Parameter.RefreshToken, value },
			{ Parameter.ClientId, value },
			{ Parameter.ClientSecret, value },
			{ Parameter.ClientAssertion, value },
			{ Parameter.ClientAssertionType, value },
            { Parameter.DPoP, value }
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.GrantType);
		Assert.Equal(expectedValue, request.Code);
		Assert.Equal(expectedValue, request.CodeVerifier);
		Assert.Equal(expectedValue, request.RedirectUri);
		Assert.Equal(expectedValue, request.RefreshToken);
		Assert.Equal(expectedValue, request.DPoPToken);
		Assert.Empty(request.ClientAuthentications);
	}

	[Fact]
	public async Task GetRequest_NormalStringParameters_ExpectValues()
	{
		// Arrange
		const string value = "random_value";
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.GrantType, value },
			{ Parameter.Code, value },
			{ Parameter.CodeVerifier, value },
			{ Parameter.RedirectUri, value },
			{ Parameter.RefreshToken, value },
			{ Parameter.ClientId, value },
            { Parameter.DPoP, value }
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent),
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(value, request.GrantType);
		Assert.Equal(value, request.Code);
		Assert.Equal(value, request.CodeVerifier);
		Assert.Equal(value, request.RedirectUri);
		Assert.Equal(value, request.RefreshToken);

        Assert.Single(request.ClientAuthentications);
        Assert.Equal(value, request.ClientAuthentications.OfType<ClientIdAuthentication>().Single().ClientId);
        Assert.Equal(TokenEndpointAuthMethod.None, request.ClientAuthentications.OfType<ClientIdAuthentication>().Single().Method);
    }

    [Fact]
    public async Task GetRequest_NormalStringParametersForClientAuthentication_ExpectValues()
    {
        // Arrange
        const string value = "random_value";
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
        var formUrlContent = new Dictionary<string, StringValues>
        {
            { Parameter.ClientId, value },
            { Parameter.ClientSecret, value },
            { Parameter.ClientAssertion, value },
            { Parameter.ClientAssertionType, value }
        };

        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Form = new FormCollection(formUrlContent),
                Headers =
                {
                    Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{value.FormUrlEncode()}:{value.FormUrlEncode()}"))}"
                }
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Collection(request.ClientAuthentications,
            clientAuthentication =>
            {
                Assert.IsType<ClientSecretAuthentication>(clientAuthentication);
                var clientSecretAuthentication = (clientAuthentication as ClientSecretAuthentication)!;
                Assert.Equal(value, clientSecretAuthentication.ClientId);
                Assert.Equal(value, clientSecretAuthentication.ClientSecret);
                Assert.Equal(TokenEndpointAuthMethod.ClientSecretBasic, clientSecretAuthentication.Method);
            },
            clientAuthentication =>
            {
                Assert.IsType<ClientSecretAuthentication>(clientAuthentication);
                var clientSecretAuthentication = (clientAuthentication as ClientSecretAuthentication)!;
                Assert.Equal(value, clientSecretAuthentication.ClientId);
                Assert.Equal(value, clientSecretAuthentication.ClientSecret);
                Assert.Equal(TokenEndpointAuthMethod.ClientSecretPost, clientSecretAuthentication.Method);
            },
            clientAuthentication =>
            {
                Assert.IsType<ClientAssertionAuthentication>(clientAuthentication);
                var clientAssertionAuthentication = (clientAuthentication as ClientAssertionAuthentication)!;
                Assert.Equal(value, clientAssertionAuthentication.ClientId);
                Assert.Equal(value, clientAssertionAuthentication.ClientAssertion);
                Assert.Equal(value, clientAssertionAuthentication.ClientAssertionType);
                Assert.Equal(ClientTokenAudience.TokenEndpoint, clientAssertionAuthentication.Audience);
                Assert.Equal(TokenEndpointAuthMethod.PrivateKeyJwt, clientAssertionAuthentication.Method);
            });
    }

    [Fact]
	public async Task GetRequest_SpaceDelimitedParameters_ExpectValues()
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		const string value = "three random values";
		string[] expectedValue = ["three", "random", "values"];
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Scope, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.Scope);
	}

	[Theory]
	[InlineData("", 0)]
	[InlineData(null, 0)]
	public async Task GetRequest_SpaceDelimitedParameters_ExpectZeroValues(string? value, int expectedCount)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Scope, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedCount, request.Scope.Count);
	}

	[Fact]
    public async Task GetRequest_CollectionParameters_ExpectValues()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
        var values = new StringValues(["three", "random", "values"]);
        string[] expectedValue = ["three", "random", "values"];
        var formUrlContent = new Dictionary<string, StringValues>
        {
            { Parameter.Resource, values },
        };

        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Form = new FormCollection(formUrlContent)
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.Resource);
    }

	[Theory]
	[InlineData("", 0)]
	[InlineData(null, 0)]
    public async Task GetRequest_CollectionParameters_ExpectZeroValues(string? value, int expectedCount)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
        var formUrlContent = new Dictionary<string, StringValues>
        {
            { Parameter.Resource, value },
        };

        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Form = new FormCollection(formUrlContent)
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedCount, request.Resource.Count);
    }
}