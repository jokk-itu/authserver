using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class AuthorizeRequestAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Theory]
    [InlineData("random_value", "random_value")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public async Task GetRequest_NormalStringParametersQuery_ExpectValues(string? value, string? expectedValue)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "GET",
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.LoginHint, value },
                    { Parameter.Display, value },
                    { Parameter.ResponseMode, value },
                    { Parameter.MaxAge, value },
                    { Parameter.ClientId, value },
                    { Parameter.CodeChallenge, value },
                    { Parameter.CodeChallengeMethod, value },
                    { Parameter.RedirectUri, value },
                    { Parameter.IdTokenHint, value },
                    { Parameter.Prompt, value },
                    { Parameter.ResponseType, value },
                    { Parameter.Nonce, value },
                    { Parameter.State, value },
                    { Parameter.RequestUri, value},
                    { Parameter.Request, value }
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.LoginHint);
        Assert.Equal(expectedValue, request.Display);
        Assert.Equal(expectedValue, request.ResponseMode);
        Assert.Equal(expectedValue, request.MaxAge);
        Assert.Equal(expectedValue, request.ClientId);
        Assert.Equal(expectedValue, request.CodeChallenge);
        Assert.Equal(expectedValue, request.CodeChallengeMethod);
        Assert.Equal(expectedValue, request.RedirectUri);
        Assert.Equal(expectedValue, request.IdTokenHint);
        Assert.Equal(expectedValue, request.Prompt);
        Assert.Equal(expectedValue, request.ResponseType);
        Assert.Equal(expectedValue, request.Nonce);
        Assert.Equal(expectedValue, request.State);
        Assert.Equal(expectedValue, request.RequestObject);
        Assert.Equal(expectedValue, request.RequestUri);
    }

    [Theory]
    [InlineData("random_value", "random_value")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public async Task GetRequest_NormalStringParametersBody_ExpectValues(string? value, string? expectedValue)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.LoginHint, value },
                    { Parameter.Display, value },
                    { Parameter.ResponseMode, value },
                    { Parameter.MaxAge, value },
                    { Parameter.ClientId, value },
                    { Parameter.CodeChallenge, value },
                    { Parameter.CodeChallengeMethod, value },
                    { Parameter.RedirectUri, value },
                    { Parameter.IdTokenHint, value },
                    { Parameter.Prompt, value },
                    { Parameter.ResponseType, value },
                    { Parameter.Nonce, value },
                    { Parameter.State, value },
                    { Parameter.RequestUri, value},
                    { Parameter.Request, value }
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.LoginHint);
        Assert.Equal(expectedValue, request.Display);
        Assert.Equal(expectedValue, request.ResponseMode);
        Assert.Equal(expectedValue, request.MaxAge);
        Assert.Equal(expectedValue, request.ClientId);
        Assert.Equal(expectedValue, request.CodeChallenge);
        Assert.Equal(expectedValue, request.CodeChallengeMethod);
        Assert.Equal(expectedValue, request.RedirectUri);
        Assert.Equal(expectedValue, request.IdTokenHint);
        Assert.Equal(expectedValue, request.Prompt);
        Assert.Equal(expectedValue, request.ResponseType);
        Assert.Equal(expectedValue, request.Nonce);
        Assert.Equal(expectedValue, request.State);
        Assert.Equal(expectedValue, request.RequestObject);
        Assert.Equal(expectedValue, request.RequestUri);
    }

    [Fact]
    public async Task GetRequest_SpaceDelimitedParametersQuery_ExpectValues()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        const string value = "three random values";
        string[] expectedValue = [ "three", "random", "values" ];
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "GET",
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.Scope);
        Assert.Equal(expectedValue, request.AcrValues);
    }

    [Fact]
    public async Task GetRequest_SpaceDelimitedParametersBody_ExpectValues()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        const string value = "three random values";
        string[] expectedValue = ["three", "random", "values"];
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.Scope);
        Assert.Equal(expectedValue, request.AcrValues);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public async Task GetRequest_SpaceDelimitedParametersQuery_ExpectZeroValues(string? value, int expectedCount)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "GET",
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedCount, request.Scope.Count);
        Assert.Equal(expectedCount, request.AcrValues.Count);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public async Task GetRequest_SpaceDelimitedParametersBody_ExpectZeroValues(string? value, int expectedCount)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<AuthorizeRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedCount, request.Scope.Count);
        Assert.Equal(expectedCount, request.AcrValues.Count);
    }
}