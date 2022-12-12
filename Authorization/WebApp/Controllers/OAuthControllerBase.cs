using Application;
using Microsoft.AspNetCore.Http.Extensions;
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
            response.Error = error;

        if (!string.IsNullOrWhiteSpace(errorDescription))
            response.ErrorDescription = errorDescription;

        return BadRequest(response);
    }

    protected IActionResult RedirectOAuthResult(string redirectUri, string state, string? error, string? errorDescription)
    {
        var queryBuilder = new QueryBuilder
        {
          { ParameterNames.State, state },
          { ParameterNames.Issuer, _identityConfiguration.Issuer}
        };
        if (!string.IsNullOrWhiteSpace(error))
        {
            queryBuilder.Add(ParameterNames.Error, error);
        }

        if (!string.IsNullOrWhiteSpace(errorDescription))
        {
            queryBuilder.Add(ParameterNames.ErrorDescription, errorDescription);
        }

        return Redirect($"{redirectUri}{queryBuilder.ToQueryString()}");
    }

    protected IActionResult AuthorizationCodeFormPostResult(string redirectUri, string state, string code)
    {
        return new ContentResult
        {
            ContentType = "text/html",
            Content = FormPostBuilder.BuildAuthorizationCodeResponse(redirectUri, state, code, _identityConfiguration.Issuer)
        };
    }

    protected IActionResult ErrorFormPostResult(string redirectUri, string state, string? error, string? errorDescription)
    {
      return new ContentResult
      {
        ContentType = "text/html",
        Content = FormPostBuilder.BuildErrorResponse(redirectUri, state, error, errorDescription, _identityConfiguration.Issuer)
      };
    }
}