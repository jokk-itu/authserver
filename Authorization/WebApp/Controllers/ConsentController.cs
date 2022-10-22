using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : Controller
{
  public ConsentController()
  {
      
  }

  [HttpGet]
  [Authorize(Policy = AuthorizationConstants.Consent)]
  public async Task<IActionResult> Index()
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    // TODO get claims from requested scopes
    throw new NotImplementedException();
  }
}
