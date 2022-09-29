using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Contracts.PostToken;
public record PostTokenRequest
{
  [FromForm(Name = ParameterNames.GrantType)]
  public string GrantType { get; init; } = null!;

  [FromForm(Name = ParameterNames.Code)] 
  public string Code { get; init; } = null!;

  [FromForm(Name = ParameterNames.ClientId)]
  public string ClientId { get; init; } = null!;

  [FromForm(Name = ParameterNames.ClientSecret)]
  public string ClientSecret { get; init; } = null!;

  [FromForm(Name = ParameterNames.RedirectUri)]
  public string RedirectUri { get; init; } = null!;

  [FromForm(Name = ParameterNames.Scope)]
  public string Scope { get; init; } = null!;

  [FromForm(Name = ParameterNames.CodeVerifier)]
  public string CodeVerifier { get; init; } = null!;

  [FromForm(Name = ParameterNames.RefreshToken)]
  public string RefreshToken { get; init; } = null!;
}