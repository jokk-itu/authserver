using System.Text;
using System.Text.Json;
using AuthServer.EndSession.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace AuthServer.EndSession;

internal class EndSessionUserAccessor : IEndSessionUserAccessor
{
    private const string EndSessionUserCookieName = "EndSessionUser";

    private readonly CookieOptions _cookieOptions;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtector _dataProtector;

    public EndSessionUserAccessor(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("EndSessionUserCookie");
        _httpContextAccessor = httpContextAccessor;

        _cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(5)
        };
    }

    public EndSessionUser GetUser() => InternalTryGetUser() ?? throw new InvalidOperationException("EndSessionUser is not set");

    public EndSessionUser? TryGetUser() => InternalTryGetUser();

    public void SetUser(EndSessionUser endSessionUser)
    {
        if (InternalTryGetUser() is not null)
        {
            throw new InvalidOperationException("EndSessionUser is already set");
        }

        InternalSetUser(endSessionUser);
    }

    public bool TrySetUser(EndSessionUser endSessionUser)
    {
        if (InternalTryGetUser() is not null)
        {
            return false;
        }

        InternalSetUser(endSessionUser);
        return true;
    }

    public bool ClearUser()
    {
        if (InternalTryGetUser() is null)
        {
            return false;
        }

        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(EndSessionUserCookieName, _cookieOptions);
        return true;
    }

    private EndSessionUser? InternalTryGetUser()
    {
        var hasEndSessionUserCookie = _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(EndSessionUserCookieName, out var encryptedEndSessionUser);
        if (!hasEndSessionUserCookie)
        {
            return null;
        }

        var decryptedEndSessionUser = _dataProtector.Unprotect(Convert.FromBase64String(encryptedEndSessionUser!));
        var endSessionUser = JsonSerializer.Deserialize<EndSessionUser>(Encoding.UTF8.GetString(decryptedEndSessionUser));
        return endSessionUser!;
    }

    private void InternalSetUser(EndSessionUser endSessionUser)
    {
        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(endSessionUser));
        var encryptedAuthorizeUser = _dataProtector.Protect(authorizeUserBytes);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(EndSessionUserCookieName, Convert.ToBase64String(encryptedAuthorizeUser), _cookieOptions);
    }
}