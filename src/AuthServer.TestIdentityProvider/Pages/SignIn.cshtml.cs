using System.Security.Claims;
using System.Web;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.TestIdentityProvider.Pages;

[ValidateAntiForgeryToken]
public class SignInModel : PageModel
{
    private readonly IAuthorizeUserAccessor _authorizeUserAccessor;
    private readonly IAuthorizeService _authorizeService;

    public SignInModel(
        IAuthorizeUserAccessor authorizeUserAccessor,
        IAuthorizeService authorizeService)
    {
        _authorizeUserAccessor = authorizeUserAccessor;
        _authorizeService = authorizeService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [BindProperty(Name = "returnUrl", SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public void OnGet(string returnUrl)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPost(string returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input is { Username: UserConstants.Username, Password: UserConstants.Password })
        {
            var query = HttpUtility.ParseQueryString(new Uri(ReturnUrl).Query);
            await _authorizeService.CreateAuthorizationGrant(UserConstants.SubjectIdentifier, query.Get(Parameter.ClientId)!, [AuthenticationMethodReferenceConstants.Password], cancellationToken);
            _authorizeUserAccessor.SetUser(new AuthorizeUser(UserConstants.SubjectIdentifier));

            var claimsIdentity = new ClaimsIdentity(
                [new Claim(ClaimNameConstants.Sub, UserConstants.SubjectIdentifier)],
                CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            var prompt = query.Get(Parameter.Prompt);
            if (prompt?.Contains(PromptConstants.Consent) == true)
            {
                return Redirect($"/Consent?returnUrl={returnUrl}");
            }
            
            return Redirect(ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}