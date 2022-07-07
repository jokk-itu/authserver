namespace AuthorizationServer.Tokens;

public record AuthorizationCode
{
  public string ClientId { get; init; }

  public string RedirectUri { get; init; }

  public ICollection<string> Scopes { get; init; }

  public string CodeChallenge { get; init; }

  public string Nonce { get; init; }

  public string UserId { get; init; }
}