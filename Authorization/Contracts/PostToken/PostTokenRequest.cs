using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts.PostToken;
public class PostTokenRequest
{
  public string GrantType { get; set; }

  public string? Code { get; set; }

  public string? ClientId { get; set; }

  public string? ClientSecret { get; set; }

  public string? RedirectUri { get; set; }

  public string? Scope { get; set; }

  public string? CodeVerifier { get; set; }

  public string? RefreshToken { get; set; }
}