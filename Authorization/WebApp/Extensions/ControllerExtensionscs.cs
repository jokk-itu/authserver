using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Extensions;

public static class ControllerExtensionscs
{
  public static IActionResult BadOAuthRequest(this ControllerBase controller, string error, string errorDescription)
  {
    return controller.BadRequest(new ErrorResponse 
    {
      Error = error,
      ErrorDescription = errorDescription
    });
  }

  public static IActionResult BadOAuthRequest(this ControllerBase controller, string error)
  {
    return controller.BadRequest(new ErrorResponse
    {
      Error = error
    });
  }
}
