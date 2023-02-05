using System.Net;
using Application;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts;

namespace WebApp.Controllers.Abstracts;

public abstract class OAuthControllerBase : Controller
{
    private readonly IdentityConfiguration _identityConfiguration;

    protected OAuthControllerBase(IdentityConfiguration identityConfiguration)
    {
        _identityConfiguration = identityConfiguration;
    }

    protected IActionResult BadOAuthResult(string? error, string? errorDescription)
    {
        var response = new ErrorResponse();
        if (!string.IsNullOrWhiteSpace(error))
        {
            response.Error = error;
        }

        if (!string.IsNullOrWhiteSpace(errorDescription))
        {
            response.ErrorDescription = errorDescription;
        }

        return BadRequest(response);
    }

    protected IActionResult AuthorizationCodeFormPostResult(string redirectUri, string state, string code)
    {
        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
        {
            throw new ArgumentException($"{nameof(redirectUri)} must be a well formed uri");
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException($"{nameof(state)} must not be null or whitespace");
        }

        return new ContentResult
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = MimeTypeConstants.Html,
            Content = FormPostBuilder.BuildAuthorizationCodeResponse(redirectUri, state, code, _identityConfiguration.Issuer)
        };
    }

    protected IActionResult ErrorFormPostResult(string redirectUri, string state, string? error, string? errorDescription)
    {
        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
        {
            throw new ArgumentException($"{nameof(redirectUri)} must be a well formed uri");
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException($"{nameof(state)} must not be null or whitespace");
        }

        return new ContentResult
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = MimeTypeConstants.Html,
            Content = FormPostBuilder.BuildErrorResponse(redirectUri, state, error, errorDescription, _identityConfiguration.Issuer)
        };
    }
}