using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;

#nullable disable
public class RedeemClientCredentialsGrantCommand : IRequest<RedeemClientCredentialsGrantResponse>
{
  public string GrantType { get; init; }
  public ICollection<ClientAuthentication> ClientAuthentications { get; init; } = new List<ClientAuthentication>();
  public string Scope { get; init; }
  public ICollection<string> Resource { get; init; }
}