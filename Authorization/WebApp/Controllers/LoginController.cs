using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class LoginController : Controller
{
  public LoginController()
  {
      
  }

  public IActionResult Index()
  {
    return View();
  }
}
