using MediatR;

namespace Infrastructure.Requests.GetLoginToken;

#nullable disable
public class GetLoginTokenQuery : IRequest<GetLoginTokenResponse>
{
  public string Username { get; init; }
  public string Password { get; init; }
}