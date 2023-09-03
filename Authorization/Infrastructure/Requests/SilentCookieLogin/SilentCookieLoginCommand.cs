using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.SilentCookieLogin;

#nullable disable
public class SilentCookieLoginCommand : AuthorizeRequest, IRequest<SilentCookieLoginResponse>
{
  public string UserId { get; set; }
}