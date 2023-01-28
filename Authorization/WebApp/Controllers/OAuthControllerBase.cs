using System.Net;
using Application;
using Infrastructure.Requests;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts;

namespace WebApp.Controllers;

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
      return new ContentResult
      {
        StatusCode = (int)HttpStatusCode.OK,
        ContentType = MimeTypeConstants.Html,
        Content = FormPostBuilder.BuildAuthorizationCodeResponse(redirectUri, state, code, _identityConfiguration.Issuer)
      };
    }

    protected IActionResult ErrorFormPostResult(string redirectUri, string state, string? error, string? errorDescription)
    {
      return new ContentResult
      {
        StatusCode = (int)HttpStatusCode.OK,
        ContentType = MimeTypeConstants.Html,
        Content = FormPostBuilder.BuildErrorResponse(redirectUri, state, error, errorDescription, _identityConfiguration.Issuer)
      };
    }
}