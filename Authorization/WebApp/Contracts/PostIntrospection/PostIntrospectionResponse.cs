using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApp.Constants;

#nullable enable

namespace WebApp.Contracts.PostIntrospection;

public class PostIntrospectionResponse
{
  [FromForm(Name = ParameterNames.Active)]
  public bool Active { get; init; }

  [FromForm(Name = ParameterNames.Scope)]
  public string? Scope { get; init; }

  [FromForm(Name = ParameterNames.ClientId)]
  public string? ClientId { get; init; }

  [FromForm(Name = ParameterNames.Username)]
  public string? Username { get; init; }

  [FromForm(Name = ParameterNames.TokenType)]
  public string? TokenType { get; init; }

  [FromForm(Name = ParameterNames.Expires)]
  public long? ExpiresAt { get; init; }

  [FromForm(Name = ParameterNames.IssuedAt)]
  public long? IssuedAt { get; init; }

  [FromForm(Name = ParameterNames.NotBefore)]
  public long? NotBefore { get; init; }

  [FromForm(Name = ParameterNames.Subject)]
  public string? Subject { get; init; }

  [FromForm(Name = ParameterNames.Audience)]
  public IEnumerable<string> Audience { get; init; } = new List<string>();

  [FromForm(Name = ParameterNames.Issuer)]
  public string? Issuer { get; init; }

  [FromForm(Name = ParameterNames.JwtId)]
  public string? JwtId { get; init; }
}