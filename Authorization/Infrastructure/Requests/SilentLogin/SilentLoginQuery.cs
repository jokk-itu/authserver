using MediatR;

#nullable disable
namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginQuery : IRequest<SilentLoginResponse>
{
  public string IdTokenHint { get; init; }
  public string ClientId { get; init; }
}
