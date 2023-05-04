using MediatR;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;

#nullable disable
public class RedeemRefreshTokenGrantCommand : IRequest<RedeemRefreshTokenGrantResponse>
{
  public string ClientId { get; init; } 

  public string ClientSecret { get; init; }

  public string RefreshToken { get; init; }

  public string GrantType { get; init; }

  public string Scope { get; init; }
}
