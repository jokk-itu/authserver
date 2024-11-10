using System.Text;
using AuthServer.Authentication.Models;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Introspection;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class IntrospectionRequesterAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
	[Theory]
	[InlineData("", "")]
	[InlineData(null, null)]
	public async Task GetRequest_NormalStringParameters_ExpectNoValues(string? value, string? expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<IntrospectionRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Token, value },
			{ Parameter.TokenTypeHint, value },
			{ Parameter.ClientId, value },
			{ Parameter.ClientSecret, value },
			{ Parameter.ClientAssertion, value },
			{ Parameter.ClientAssertionType, value },
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
		Assert.Equal(expectedValue, request.Token);
		Assert.Equal(expectedValue, request.TokenTypeHint);
		Assert.Empty(request.ClientAuthentications);
	}

	[Fact]
	public async Task GetRequest_NormalStringParameters_ExpectValues()
	{
		// Arrange
        const string value = "random_value";
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<IntrospectionRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Token, value },
			{ Parameter.TokenTypeHint, value },
			{ Parameter.ClientId, value },
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
		Assert.Equal(value, request.Token);
		Assert.Equal(value, request.TokenTypeHint);
        Assert.Empty(request.ClientAuthentications);
    }

    [Fact]
    public async Task GetRequest_NormalStringParametersForClientAuthentication_ExpectValues()
    {
		// Arrange
        const string value = "random_value";
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<IntrospectionRequest>>();
        var formUrlContent = new Dictionary<string, StringValues>
        {
            { Parameter.ClientId, value },
            { Parameter.ClientSecret, value },
            { Parameter.ClientAssertion, value },
            { Parameter.ClientAssertionType, value },
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
                Assert.Equal(ClientTokenAudience.IntrospectionEndpoint, clientAssertionAuthentication.Audience);
                Assert.Equal(TokenEndpointAuthMethod.PrivateKeyJwt, clientAssertionAuthentication.Method);
            });
    }
}