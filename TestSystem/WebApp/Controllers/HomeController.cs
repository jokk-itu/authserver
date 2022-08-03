using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;
public class HomeController : Controller
{
  private readonly WebApiService _webApiService;

  public HomeController(WebApiService webApiService)
  {
    _webApiService = webApiService;
  }

  public IActionResult Index()
  {
    return View();
  }

  [Authorize]
  public async Task<IActionResult> Secret()
  {
    var secret = await _webApiService.GetSecretAsync();
    return View(new SecretModel { Secret = secret });
  }

  [AllowAnonymous]
  public async Task<IActionResult> Anonymous()
  {
    var anonymous = await _webApiService.GetAnonymousAsync();
    return View(new AnonymousModel { Anonymous = anonymous });
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
