using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;

#nullable disable
public class RedeemClientCredentialsGrantCommand : IRequest<RedeemClientCredentialsGrantResponse>
{
  public string GrantType { get; init; }
  public string ClientId { get; init; }
  public string ClientSecret { get; init; }
  public string Scope { get; init; }
}