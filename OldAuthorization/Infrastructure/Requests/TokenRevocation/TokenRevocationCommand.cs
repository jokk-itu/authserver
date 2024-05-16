using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.TokenRevocation;

#nullable disable
public class TokenRevocationCommand : IRequest<TokenRevocationResponse>
{
  public string Token { get; init; }
  public string TokenTypeHint { get; init; }
  public ICollection<ClientAuthentication> ClientAuthentications { get; init; } = new List<ClientAuthentication>();
}