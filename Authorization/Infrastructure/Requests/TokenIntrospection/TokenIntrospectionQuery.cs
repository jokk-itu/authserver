using MediatR;

namespace Infrastructure.Requests.TokenIntrospection;

#nullable disable
public class TokenIntrospectionQuery : IRequest<TokenIntrospectionResponse>
{
  public string Token { get; init; }
  public string TokenTypeHint { get; init; }
  public string ClientId { get; init; }
  public string ClientSecret { get; init; }
}