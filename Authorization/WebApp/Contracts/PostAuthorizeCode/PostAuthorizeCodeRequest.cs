using Microsoft.AspNetCore.Mvc;
namespace Contracts.AuthorizeCode;
#nullable disable
public record PostAuthorizeCodeRequest
{
  [FromForm(Name = "username")]
  public string Username { get; init; }

  [FromForm(Name = "password")]
  public string Password { get; init; }
}