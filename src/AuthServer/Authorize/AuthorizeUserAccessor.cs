using System.Text;
using System.Text.Json;
using AuthServer.Authorize.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Authorize;

internal class AuthorizeUserAccessor : IAuthorizeUserAccessor
{
    private const string AuthorizeUserCookieName = "AuthorizeUser";

    private readonly CookieOptions _cookieOptions;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtector _dataProtector;

    public AuthorizeUserAccessor(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("AuthorizeUserCookie");
        _httpContextAccessor = httpContextAccessor;

        _cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(5),
        };
    }

    public AuthorizeUser GetUser() => InternalTryGetUser() ?? throw new InvalidOperationException("authorizeUser is not set");

    public AuthorizeUser? TryGetUser() => InternalTryGetUser();

    public void SetUser(AuthorizeUser authorizeUser)
    {
        if (InternalTryGetUser() is not null)
        {
            throw new InvalidOperationException("authorizerUser is already set");
        }

        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authorizeUser));
        var encryptedAuthorizeUser = _dataProtector.Protect(authorizeUserBytes);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(AuthorizeUserCookieName, Encoding.UTF8.GetString(encryptedAuthorizeUser), _cookieOptions);
    }

    public bool TrySetUser(AuthorizeUser authorizeUser)
    {
        if (InternalTryGetUser() is not null)
        {
            return false;
        }

        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authorizeUser));
        var encryptedAuthorizeUser = _dataProtector.Protect(authorizeUserBytes);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(AuthorizeUserCookieName, Encoding.UTF8.GetString(encryptedAuthorizeUser), _cookieOptions);
        return true;
    }

    public bool ClearUser()
    {
        if (InternalTryGetUser() is null)
        {
            return false;
        }

        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(AuthorizeUserCookieName, _cookieOptions);
        return true;
    }

    private AuthorizeUser? InternalTryGetUser()
    {
        var hasAuthorizeUserCookie = _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(AuthorizeUserCookieName, out var encryptedAuthorizeUser);
        if (!hasAuthorizeUserCookie)
        {
            return null;
        }

        var decryptedAuthorizeUser = _dataProtector.Unprotect(Encoding.UTF8.GetBytes(encryptedAuthorizeUser!));
        var authenticatedUser = JsonSerializer.Deserialize<AuthorizeUser>(decryptedAuthorizeUser);
        return authenticatedUser!;
    }
}