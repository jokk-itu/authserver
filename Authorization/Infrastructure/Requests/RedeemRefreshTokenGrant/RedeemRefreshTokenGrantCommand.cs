using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;

#nullable disable
public class RedeemRefreshTokenGrantCommand : IRequest<RedeemRefreshTokenGrantResponse>
{
  public ICollection<ClientAuthentication> ClientAuthentications { get; init; } = new List<ClientAuthentication>();

  public string RefreshToken { get; init; }

  public string GrantType { get; init; }

  public string Scope { get; init; }
  public ICollection<string> Resource { get; init; }
}
