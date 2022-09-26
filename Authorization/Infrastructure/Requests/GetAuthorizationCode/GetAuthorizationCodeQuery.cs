using MediatR;

namespace Infrastructure.Requests.GetAuthorizationCode;

#nullable disable
public class GetAuthorizationCodeQuery : IRequest<GetAuthorizationCodeResponse>
{
  public string Username { get; init; }

  public string Password { get; init; }

  public string ResponseType { get; init; }

  public string ClientId { get; init; }

  public string RedirectUri { get; init; }

  public ICollection<string> Scopes { get; init; }

  public string State { get; init; }

  public string CodeChallenge { get; init; }

  public string CodeChallengeMethod { get; init; }

  public string Nonce { get; init; }
}
