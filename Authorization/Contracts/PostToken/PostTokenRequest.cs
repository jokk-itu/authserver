using Microsoft.AspNetCore.Mvc;

namespace Contracts.PostToken;
public class PostTokenRequest
{
  [FromForm(Name = "grant_type")]
  public string GrantType { get; set; } = null!;

  [FromForm(Name = "code")]
  public string? Code { get; set; }

  [FromForm(Name = "client_id")]
  public string ClientId { get; set; } = null!;

  [FromForm(Name = "client_secret")]
  public string ClientSecret { get; set; } = null!;

  [FromForm(Name = "redirect_uri")]
  public string? RedirectUri { get; set; }

  [FromForm(Name = "scope")]
  public string? Scope { get; set; }

  [FromForm(Name = "code_verifier")]
  public string? CodeVerifier { get; set; }

  [FromForm(Name = "refresh_token")]
  public string? RefreshToken { get; set; }
}