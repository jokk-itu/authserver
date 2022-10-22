using Contracts;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Extensions;

public static class ControllerExtensions
{
  public static IActionResult BadOAuthResult(this ControllerBase controller, string? error, string? errorDescription)
  {
    var response = new ErrorResponse();

    if (!string.IsNullOrWhiteSpace(error))
      response.Error = error;

    if (!string.IsNullOrWhiteSpace(errorDescription))
      response.ErrorDescription = errorDescription;

    return controller.BadRequest(response);
  }

  public static IActionResult RedirectOAuthResult(this ControllerBase controller, string redirectUri, string state, string? error, string? errorDescription)
  {
    var queryBuilder = new QueryBuilder
    {
      { ParameterNames.State, state },
    };
    if(!string.IsNullOrWhiteSpace(error))
      queryBuilder.Add(ParameterNames.Error, error);

    if(!string.IsNullOrWhiteSpace(errorDescription))
      queryBuilder.Add(ParameterNames.ErrorDescription, errorDescription);

    return controller.Redirect($"{redirectUri}{queryBuilder.ToQueryString()}");
  }
}
