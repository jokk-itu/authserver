using Microsoft.AspNetCore.Mvc;

namespace Contracts.PostToken;
public record PostTokenRequest
{
  [FromForm(Name = "grant_type")]
  public string GrantType { get; init; } = null!;

  [FromForm(Name = "code")]
  public string? Code { get; init; }

  [FromForm(Name = "client_id")]
  public string ClientId { get; init; } = null!;

  [FromForm(Name = "client_secret")]
  public string ClientSecret { get; init; } = null!;

  [FromForm(Name = "redirect_uri")]
  public string? RedirectUri { get; init; }

  [FromForm(Name = "scope")]
  public string? Scope { get; init; }

  [FromForm(Name = "code_verifier")]
  public string? CodeVerifier { get; init; }

  [FromForm(Name = "refresh_token")]
  public string? RefreshToken { get; init; }
}