using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using Application;
using Domain.Constants;
using WebApp.Extensions;
using WebApp.Attributes;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class AuthorizeController : OAuthControllerBase
{
  public AuthorizeController(IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    
  }

  [HttpGet]
  [SecurityHeader]
  public IActionResult Get(
    [FromQuery(Name = ParameterNames.Prompt)] string prompt)
  {
    if (PromptConstants.Prompts.All(x => x != prompt))
    {
      return BadOAuthResult(ErrorCode.InvalidRequest, "prompt is invalid");
    }

    var prompts = prompt.Split(' ');
    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    if (prompts.Contains(PromptConstants.Create))
    {
      return RedirectToAction(controllerName: "Register", actionName: "Index", routeValues: routeValues);
    }

    if (prompts.Contains(PromptConstants.Login))
    {
      return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
    }

    return BadOAuthResult(ErrorCode.LoginRequired, "prompt must contain login");
  }
}