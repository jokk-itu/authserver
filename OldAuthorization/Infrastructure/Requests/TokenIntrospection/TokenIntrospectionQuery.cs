using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.TokenIntrospection;

#nullable disable
public class TokenIntrospectionQuery : IRequest<TokenIntrospectionResponse>
{
  public string Token { get; init; }
  public string TokenTypeHint { get; init; }
  public ICollection<ClientAuthentication> ClientAuthentications { get; init; } = new List<ClientAuthentication>();
}