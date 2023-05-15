using Microsoft.AspNetCore.Mvc;

namespace Api;
public class IntrospectionResponse
{
  [FromForm(Name = "active")]
  public bool Active { get; init; }

  [FromForm(Name = "scope")]
  public string? Scope { get; init; }

  [FromForm(Name = "client_id")]
  public string? ClientId { get; init; }

  [FromForm(Name = "username")]
  public string? Username { get; init; }

  [FromForm(Name = "token_type")]
  public string? TokenType { get; init; }

  [FromForm(Name = "exp")]
  public long? ExpiresAt { get; init; }

  [FromForm(Name = "iat")]
  public long? IssuedAt { get; init; }

  [FromForm(Name = "nbf")]
  public long? NotBefore { get; init; }

  [FromForm(Name = "sub")]
  public string? Subject { get; init; }

  [FromForm(Name = "aud")]
  public IEnumerable<string> Audience { get; init; } = new List<string>();

  [FromForm(Name = "iss")]
  public string? Issuer { get; init; }

  [FromForm(Name = "jwt_id")]
  public string? JwtId { get; init; }
}