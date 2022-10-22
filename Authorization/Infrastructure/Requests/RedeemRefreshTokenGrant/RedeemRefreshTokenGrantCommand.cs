using MediatR;

namespace Infrastructure.Requests.CreateRefreshTokenGrant;

#nullable disable
public class RedeemRefreshTokenGrantCommand : IRequest<RedeemRefreshTokenGrantResponse>
{
  public string GrantType { get; init; }

  public string ClientId { get; init; } 

  public string ClientSecret { get; init; }

  public string RefreshToken { get; init; }
}
