using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Web;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorize;

public class AuthorizeUserAccessorTest : BaseUnitTest
{
    public AuthorizeUserAccessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public void SetUser_UserAlreadySet_ExpectInvalidOperationException()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var dataProtector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("AuthorizeUserCookie");

        var authorizeUserAccessor = serviceProvider.GetRequiredService<IAuthorizeUserAccessor>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        var authorizeUser = new AuthorizeUser(Guid.NewGuid().ToString());
        var encryptedAuthorizeUser = GetEncryptedAuthorizeCookie(dataProtector, authorizeUser);
        httpContextAccessor.HttpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.Request.Cookies = HttpContextHelper.GetRequestCookies(
            new Dictionary<string, string>
            {
                { "AuthorizeUser", encryptedAuthorizeUser }
            });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => authorizeUserAccessor.SetUser(authorizeUser));
    }

    [Fact]
    public void SetUser_UserNotSet_ExpectUserIsSet()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var dataProtector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("AuthorizeUserCookie");

        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext();

        var authorizeUserAccessor = serviceProvider.GetRequiredService<IAuthorizeUserAccessor>();
        var authorizeUser = new AuthorizeUser(Guid.NewGuid().ToString());

        // Act
        authorizeUserAccessor.SetUser(authorizeUser);

        // Assert
        var headers = httpContextAccessor.HttpContext!.Response.GetTypedHeaders();
        var encryptedAuthorizeUser = HttpUtility.UrlDecode(headers.SetCookie.First().Value.Value!);
        Assert.Equal(authorizeUser.SubjectIdentifier, GetDecryptedAuthorizeCookie(dataProtector, encryptedAuthorizeUser).SubjectIdentifier);
    }

    [Fact]
    public void GetUser_UserIsNotSet_ExpectInvalidOperationException()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var dataProtector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("AuthorizeUserCookie");

        var authorizeUserAccessor = serviceProvider.GetRequiredService<IAuthorizeUserAccessor>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        var authorizeUser = new AuthorizeUser(Guid.NewGuid().ToString());
        var encryptedAuthorizeUser = GetEncryptedAuthorizeCookie(dataProtector, authorizeUser);
        httpContextAccessor.HttpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.Request.Cookies = HttpContextHelper.GetRequestCookies(
            new Dictionary<string, string>
            {
                { "AuthorizeUser", encryptedAuthorizeUser }
            });

        // Act
        var decryptedAuthorizeUser = authorizeUserAccessor.GetUser();
        
        // Assert
        Assert.Equal(authorizeUser.SubjectIdentifier, decryptedAuthorizeUser.SubjectIdentifier);
    }

    [Fact]
    public void GetUser_UserIsSet_ExpectUser()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();

        var authorizeUserAccessor = serviceProvider.GetRequiredService<IAuthorizeUserAccessor>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(authorizeUserAccessor.GetUser);
    }

    private static string GetEncryptedAuthorizeCookie(IDataProtector dataProtector, AuthorizeUser authorizeUser)
    {
        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authorizeUser));
        var encryptedAuthorizeUser = dataProtector.Protect(authorizeUserBytes);
        return Convert.ToBase64String(encryptedAuthorizeUser);
    }

    private static AuthorizeUser GetDecryptedAuthorizeCookie(IDataProtector dataProtector, string authorizeUser)
    {
        var decryptedAuthorizeUser = dataProtector.Unprotect(Convert.FromBase64String(authorizeUser));
        return JsonSerializer.Deserialize<AuthorizeUser>(Encoding.UTF8.GetString(decryptedAuthorizeUser))!;
    }
}