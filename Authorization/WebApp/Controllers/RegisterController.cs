using Application;
using Domain;
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc;
using WebApp.Attributes;
using WebApp.Contracts.PostRegisterUser;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class RegisterController : OAuthControllerBase
{
  private readonly IdentityContext _identityContext;

  public RegisterController(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _identityContext = identityContext;
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
    var user = new User
    {
      Id = Guid.NewGuid().ToString(),
      FirstName = request.GivenName,
      LastName = request.FamilyName,
      Address = request.Address,
      Locale = request.Locale,
      Birthdate = request.BirthDate,
      UserName = request.Username,
      Email = request.Email,
      PhoneNumber = request.PhoneNumber
    };
    var salt = BCrypt.GenerateSalt();
    var hashedPassword = BCrypt.HashPassword(request.Password, salt);
    user.Password = hashedPassword;
    await _identityContext.Set<User>().AddAsync(user);
    await _identityContext.SaveChangesAsync();
    return Ok();
  }
}
