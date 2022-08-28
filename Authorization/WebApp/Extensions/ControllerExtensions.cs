using Contracts;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Extensions;

public static class ControllerExtensions
{
  public static IActionResult BadOAuthResult(this ControllerBase controller, string error, string errorDescription)
  {
    return controller.BadRequest(new ErrorResponse 
    {
      Error = error,
      ErrorDescription = errorDescription
    });
  }

  public static IActionResult BadOAuthResult(this ControllerBase controller, string error)
  {
    return controller.BadRequest(new ErrorResponse
    {
      Error = error
    });
  }

  public static IActionResult RedirectOAuthResult(this ControllerBase controller, string redirectUri, string state, string error, string errorDescription)
  {
    var query = new QueryBuilder
    {
      { "state", state },
      { "error", error },
      { "error_description", errorDescription }
    }.ToQueryString();
    return controller.Redirect($"{redirectUri}{query}");
  }

  public static IActionResult RedirectOAuthResult(this ControllerBase controller, string redirectUri, string state, string error)
  {
    var query = new QueryBuilder
    {
      { "state", state },
      { "error", error }
    }.ToQueryString();
    return controller.Redirect($"{redirectUri}{query}");
  }
}
