using System.Text;
using System.Text.Json;
using AuthServer.Authorize.Abstractions;
using AuthServer.Core.Discovery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorize;

internal class UserAccessor : IUserAccessor
{
    private const string AuthenticatedUserCookieName = "AuthenticatedUser";

    private readonly CookieOptions _cookieOptions;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtector _dataProtector;

    public UserAccessor(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("AuthenticatedUserCookie");
        _httpContextAccessor = httpContextAccessor;

        _cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(5),
            Domain = discoveryDocumentOptions.Value.Issuer
        };
    }

    public AuthenticatedUser GetUser() => InternalTryGetUser() ?? throw new InvalidOperationException("authenticated user is not set");

    public AuthenticatedUser? TryGetUser() => InternalTryGetUser();

    public void SetUser(AuthenticatedUser authenticatedUser)
    {
        if (InternalTryGetUser() is not null)
        {
            throw new InvalidOperationException("authenticated user is already set");
        }

        var authenticatedUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authenticatedUser));
        var encryptedAuthenticatedUser = _dataProtector.Protect(authenticatedUserBytes);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(AuthenticatedUserCookieName, Encoding.UTF8.GetString(encryptedAuthenticatedUser), _cookieOptions);
    }

    public bool TrySetUser(AuthenticatedUser authenticatedUser)
    {
        if (InternalTryGetUser() is not null)
        {
            return false;
        }

        var authenticatedUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authenticatedUser));
        var encryptedAuthenticatedUser = _dataProtector.Protect(authenticatedUserBytes);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(AuthenticatedUserCookieName, Encoding.UTF8.GetString(encryptedAuthenticatedUser), _cookieOptions);
        return true;
    }

    public bool ClearUser()
    {
        if (InternalTryGetUser() is null)
        {
            return false;
        }

        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(AuthenticatedUserCookieName, _cookieOptions);
        return true;
    }

    private AuthenticatedUser? InternalTryGetUser()
    {
        var hasAuthenticatedUserCookie = _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(AuthenticatedUserCookieName, out var encryptedAuthenticatedUser);
        if (!hasAuthenticatedUserCookie)
        {
            return null;
        }

        var decryptedAuthenticatedUser = _dataProtector.Unprotect(Encoding.UTF8.GetBytes(encryptedAuthenticatedUser!));
        var authenticatedUser = JsonSerializer.Deserialize<AuthenticatedUser>(decryptedAuthenticatedUser);
        return authenticatedUser!;
    }
}