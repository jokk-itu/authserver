using System.Net;

namespace Infrastructure.Requests.TokenIntrospection;
public class TokenIntrospectionResponse : Response
{
  public TokenIntrospectionResponse(HttpStatusCode statusCode)
    : base(statusCode)
  {
  }

  public TokenIntrospectionResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode)
    : base(errorCode, errorDescription, statusCode)
  {
  }

  public bool Active { get; init; }
  public string? Scope { get; init; }
  public string? ClientId { get; init; }
  public string? UserName { get; init; }
  public string? TokenType { get; init; }
  public long? ExpiresAt { get; init; }
  public long? IssuedAt { get; init; }
  public long? NotBefore { get; init; }
  public string? Subject { get; init; }
  public IEnumerable<string> Audience { get; init; } = new List<string>();
  public string? Issuer { get; init; }
  public string? JwtId { get; init; }
}