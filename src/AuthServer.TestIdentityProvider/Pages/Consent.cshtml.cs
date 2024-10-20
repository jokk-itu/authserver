using AuthServer.Authorize.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using AuthServer.Core;
using AuthServer.Helpers;

namespace AuthServer.TestIdentityProvider.Pages;

[ValidateAntiForgeryToken]
public class ConsentModel : PageModel
{
    private readonly IAuthorizeUserAccessor _authorizeUserAccessor;
    private readonly IAuthorizeService _authorizeService;

    public ConsentModel(
        IAuthorizeUserAccessor authorizeUserAccessor,
        IAuthorizeService authorizeService)
    {
        _authorizeUserAccessor = authorizeUserAccessor;
        _authorizeService = authorizeService;
    }

    [BindProperty] public InputModel Input { get; set; }

    [BindProperty(Name = "returnUrl", SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        public required string ClientName { get; set; }
        public string? ClientUri { get; set; }
        public string? ClientLogoUri { get; set; }
        public required string Username { get; set; }
        public List<string> RequestedScope { get; set; } = [];
        public List<ClaimDto> RequestedClaims { get; set; } = [];
        public List<string> ConsentedScope { get; set; } = [];
        public List<string> ConsentedClaims { get; set; } = [];
    }

    public class ClaimDto
    {
        public required string Name { get; set; }
        public bool IsGranted { get; set; }
    }

    public async Task OnGet(string returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        var user = _authorizeUserAccessor.GetUser();
        var query = HttpUtility.ParseQueryString(new Uri(ReturnUrl).Query);
        var clientId = query.Get(Parameter.ClientId)!;
        var consentGrantDto =
            await _authorizeService.GetConsentGrantDto(user.SubjectIdentifier, clientId, cancellationToken);

        // Display requested scope, unless it is already consented. This makes sure the end-user only sees new scope.
        var requestedScope = query.Get(Parameter.Scope)!.Split(' ').ToList();
        if (consentGrantDto.ConsentedScope.Any())
        {
            requestedScope = requestedScope.Except(consentGrantDto.ConsentedScope).ToList();
        }

        // Display requested claims, also if they are already consented. This makes sure the end-user can change their full consent.
        var requestedClaims = ClaimsHelper.MapToClaims(requestedScope)
            .Select(x => new ClaimDto
            {
                Name = x,
                IsGranted = consentGrantDto.ConsentedClaims.Any(y => y == x)
            })
            .ToList();

        Input = new InputModel
        {
            ClientName = consentGrantDto.ClientName,
            ClientUri = consentGrantDto.ClientUri,
            ClientLogoUri = consentGrantDto.ClientLogoUri,
            Username = consentGrantDto.Username,
            RequestedScope = requestedScope,
            RequestedClaims = requestedClaims
        };
    }

    public async Task<IActionResult> OnPostAccept(string returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        var user = _authorizeUserAccessor.GetUser();
        var query = HttpUtility.ParseQueryString(new Uri(ReturnUrl).Query);
        var clientId = query.Get(Parameter.ClientId)!;

        await _authorizeService.CreateOrUpdateConsentGrant(user.SubjectIdentifier, clientId, Input.ConsentedScope, Input.ConsentedClaims, cancellationToken);

        return Redirect(ReturnUrl);
    }

    public async Task<IActionResult> OnPostDecline(string returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        // TODO redirect to the Client with "consent_required" error
        throw new NotImplementedException();
    }
}