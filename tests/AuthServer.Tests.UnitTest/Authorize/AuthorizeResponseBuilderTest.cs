using System.Web;
using AuthServer.Authorization;
using AuthServer.Authorization.Abstractions;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorize;

public class AuthorizeResponseBuilderTest : BaseUnitTest
{
    public AuthorizeResponseBuilderTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Theory]
    [InlineData(null, "https://demo.authserver.dk/request-uri/")]
    [InlineData("request-object", null)]
    public async Task BuildResponse_WithEmptyResponseModeAndRequestSubstitution_ExpectQueryRedirect(string? requestObject, string? requestUri)
    {
        // Arrange
        var authorizeRequestParameterService = new Mock<ISecureRequestService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeRequestParameterService);
        });
        var responseBuilder = serviceProvider.GetRequiredService<IAuthorizeResponseBuilder>();

        var request = new AuthorizeRequestDto
        {
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(16),
            RedirectUri = "https://demo.authserver.dk/callback"
        };
        authorizeRequestParameterService
            .Setup(x => x.GetCachedRequest())
            .Returns(request)
            .Verifiable();

        var httpContext = new DefaultHttpContext();
        var code = CryptographyHelper.GetRandomString(8);

        // Act
        var response = await responseBuilder.BuildResponse(
            new AuthorizeRequest
            {
                RequestUri = requestUri,
                RequestObject = requestObject
            },
            new Dictionary<string, string>
            {
                {"code", code}
            },
            httpContext.Response,
            CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<StatusCodeHttpResult>(response);
        
        var statusCodeResult = response as StatusCodeHttpResult;
        Assert.Equal(303, statusCodeResult!.StatusCode);

        var decodedLocation = HttpUtility.UrlDecode(httpContext.Response.Headers.Location.ToString());
        Assert.Equal($"{request.RedirectUri}?code={code}&state={request.State}", decodedLocation);
    }

    [Fact]
    public async Task BuildResponse_WithFragmentResponseWithAndEmptyRedirectUri_ExpectFragmentRedirect()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var responseBuilder = serviceProvider.GetRequiredService<IAuthorizeResponseBuilder>();

        var client = new Client("PinguWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var redirectUri = new RedirectUri("https://demo.authserver.dk/callback", client);
        await AddEntity(redirectUri);

        var httpContext = new DefaultHttpContext();
        var code = CryptographyHelper.GetRandomString(8);

        var request = new AuthorizeRequest
        {
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(16),
            ClientId = client.Id,
            ResponseMode = ResponseModeConstants.Fragment
        };

        // Act
        var response = await responseBuilder.BuildResponse(
            request,
            new Dictionary<string, string>
            {
                { "code", code }
            },
            httpContext.Response,
            CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<StatusCodeHttpResult>(response);

        var statusCodeResult = response as StatusCodeHttpResult;
        Assert.Equal(303, statusCodeResult!.StatusCode);

        var decodedLocation = HttpUtility.UrlDecode(httpContext.Response.Headers.Location.ToString());
        Assert.Equal($"{redirectUri.Uri}#code={code}&state={request.State}", decodedLocation);
    }

    [Fact]
    public async Task BuildResponse_WithFormPostResponseMode_ExpectFormPostOk()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var responseBuilder = serviceProvider.GetRequiredService<IAuthorizeResponseBuilder>();

        var httpContext = new DefaultHttpContext();
        var code = CryptographyHelper.GetRandomString(8);

        var request = new AuthorizeRequest
        {
            ResponseType = ResponseTypeConstants.Code,
            State = CryptographyHelper.GetRandomString(16),
            ResponseMode = ResponseModeConstants.FormPost,
            RedirectUri = "https://demo.authserver.dk/callback"
        };

        // Act
        var response = await responseBuilder.BuildResponse(
            request,
            new Dictionary<string, string>
            {
                { "code", code }
            },
            httpContext.Response,
            CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<ContentHttpResult>(response);

        var contentHttpResult = response as ContentHttpResult;
        Assert.Equal(200, contentHttpResult!.StatusCode);
        Assert.Equal("text/html; charset=utf-8", contentHttpResult.ContentType);

        Assert.StartsWith($"""<html><head><title>Submit Form</title></head><body onload="javascript:document.forms[0].submit()"><form method="post" action="{request.RedirectUri}">""", contentHttpResult.ResponseContent);
        Assert.Contains($"""<input type="hidden" name="code" value="{code}" />""", contentHttpResult.ResponseContent);
        Assert.Contains($"""<input type="hidden" name="iss" value="{DiscoveryDocument.Issuer}" />""", contentHttpResult.ResponseContent);
        Assert.Contains($"""<input type="hidden" name="state" value="{request.State}" />""", contentHttpResult.ResponseContent);
        Assert.EndsWith("</form></body></html>", contentHttpResult.ResponseContent);
    }
}