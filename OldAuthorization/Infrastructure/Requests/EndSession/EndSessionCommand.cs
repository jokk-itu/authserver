using MediatR;

namespace Infrastructure.Requests.EndSession;

#nullable disable
public class EndSessionCommand : IRequest<EndSessionResponse>
{
  public string IdTokenHint { get; init; }
  public string ClientId { get; init; }
  public string PostLogoutRedirectUri { get; init; }
  public string State { get; init; }
}