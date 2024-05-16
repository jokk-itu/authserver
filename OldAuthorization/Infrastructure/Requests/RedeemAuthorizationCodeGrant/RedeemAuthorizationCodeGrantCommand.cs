using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;

#nullable disable
public class RedeemAuthorizationCodeGrantCommand : IRequest<RedeemAuthorizationCodeGrantResponse>
{
  public string GrantType { get; init; }
  public string Code { get; init; }

  public ICollection<ClientAuthentication> ClientAuthentications { get; init; } = new List<ClientAuthentication>();

  public string RedirectUri { get; init; }

  public string CodeVerifier { get; init; }
  public ICollection<string> Resource { get; init; }
}
