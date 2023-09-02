using MediatR;

namespace Infrastructure.Requests.SilentCookieLogin;

#nullable disable
public class SilentCookieLoginCommand : IRequest<SilentCookieLoginResponse>
{
  public string UserId { get; set; }
  public string ClientId { get; init; }
  public string Scope { get; init; }
  public string ResponseType { get; init; }
  public string RedirectUri { get; init; }
  public string State { get; init; }
  public string CodeChallenge { get; init; }
  public string CodeChallengeMethod { get; init; }
  public string Nonce { get; init; }
  public string MaxAge { get; init; }
}