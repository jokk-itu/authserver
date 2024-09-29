using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.RequestAccessors.EndSession;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class EndSessionRequestAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Theory]
    [InlineData("random_value", "random_value")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public async Task GetRequest_GetMethodNormalStringParameters_ExpectValues(string? value, string? expectedValue)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<EndSessionRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.IdTokenHint, value },
                    { Parameter.ClientId, value },
                    { Parameter.PostLogoutRedirectUri, value },
                    { Parameter.State, value },
                }),
                Method = "GET"
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.IdTokenHint);
        Assert.Equal(expectedValue, request.ClientId);
        Assert.Equal(expectedValue, request.PostLogoutRedirectUri);
        Assert.Equal(expectedValue, request.State);
    }

    [Theory]
	[InlineData("random_value", "random_value")]
    [InlineData("", "")]
    [InlineData(null, null)]
	public async Task GetRequest_PostMethodNormalStringParameters_ExpectValues(string? value, string? expectedValue)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<EndSessionRequest>>();
        var formUrlContent = new Dictionary<string, StringValues>
        {
            { Parameter.IdTokenHint, value },
            { Parameter.ClientId, value },
            { Parameter.PostLogoutRedirectUri, value },
            { Parameter.State, value }
        };

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent),
				Method = "POST"
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.IdTokenHint);
        Assert.Equal(expectedValue, request.ClientId);
        Assert.Equal(expectedValue, request.PostLogoutRedirectUri);
        Assert.Equal(expectedValue, request.State);
    }
}