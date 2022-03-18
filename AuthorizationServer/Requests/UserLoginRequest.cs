using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Requests;

#nullable disable
public record UserLoginRequest
{
  public string Username { get; init; }
  
  [PasswordPropertyText]
  public string Password { get; init; }
}