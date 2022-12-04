using Application;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Attributes;
using WebApp.Contracts.PostRegisterUser;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class RegisterController : Controller
{
  private readonly UserManager<User> _userManager;

  public RegisterController(
    UserManager<User> userManager)
  {
    _userManager = userManager;
  }

  [HttpGet]
  [SecurityHeader]
  public IActionResult Index()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Post(PostRegisterUserRequest request)
  {
    var identityResult = await _userManager.CreateAsync(new User
    {
      FirstName = request.GivenName,
      LastName = request.FamilyName,
      Address = request.Address,
      Locale = request.Locale,
      Birthdate = request.BirthDate,
      UserName = request.Username,
      Email = request.Email,
      PhoneNumber = request.PhoneNumber,
      NormalizedEmail = _userManager.NormalizeEmail(request.Email),
      NormalizedUserName = _userManager.NormalizeName(request.Username)
    }, request.Password);

    if (identityResult.Succeeded)
      return Ok();

    return this.BadOAuthResult(ErrorCode.ServerError, "user cannot be created");
  }
}
