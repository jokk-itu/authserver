#nullable disable
using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.SilentTokenLogin;
public class SilentTokenLoginCommand : AuthorizeRequest, IRequest<SilentTokenLoginResponse>
{
  public string IdTokenHint { get; init; }
}