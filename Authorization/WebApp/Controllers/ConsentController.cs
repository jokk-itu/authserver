using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : Controller
{
  [HttpGet]
  [Authorize(Policy = AuthorizationConstants.Prompt)]
  public async Task<IActionResult> Index()
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    // TODO get claims from requested scopes
    throw new NotImplementedException();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize]
  public async Task<IActionResult> PostAsync()
  {
    // If consent is there, then update that consent

    // If consent is not there, then create consent

    // Redirect to authorize

    throw new NotImplementedException();
  }
}
