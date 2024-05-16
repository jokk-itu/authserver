using MediatR;

namespace Infrastructure.Requests.Login;

#nullable disable
public class LoginQuery : IRequest<LoginResponse>
{
  public string Username { get; init; }
  public string Password { get; init; }
}