﻿using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostClient;

#nullable disable
public class PostClientRequest
{
  [JsonPropertyName(ParameterNames.RedirectUris)]
  public ICollection<string> RedirectUris { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.PostLogoutRedirectUris)]
  public ICollection<string> PostLogoutRedirectUris { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.ResponseTypes)]
  public ICollection<string> ResponseTypes { get; init; } = new List<string>();

  [JsonPropertyName(ParameterNames.GrantTypes)]
  public ICollection<string> GrantTypes { get; init; } = new List<string>();

  [JsonPropertyName(ParameterNames.ApplicationType)]
  public string ApplicationType { get; init; }

  [JsonPropertyName(ParameterNames.Contacts)]
  public ICollection<string> Contacts { get; init; } = new List<string>();

  [JsonPropertyName(ParameterNames.ClientName)]
  public string ClientName { get; init; }

  [JsonPropertyName(ParameterNames.PolicyUri)]
  public string PolicyUri { get; init; }

  [JsonPropertyName(ParameterNames.TosUri)]
  public string TosUri { get; init; }

  [JsonPropertyName(ParameterNames.SubjectType)]
  public string SubjectType { get; init; }

  [JsonPropertyName(ParameterNames.TokenEndpointAuthMethod)]
  public string TokenEndpointAuthMethod { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; }

  [JsonPropertyName(ParameterNames.DefaultMaxAge)]
  public string DefaultMaxAge { get; init; }

  [JsonPropertyName(ParameterNames.InitiateLoginUri)]
  public string InitiateLoginUri { get; init; }

  [JsonPropertyName(ParameterNames.LogoUri)]
  public string LogoUri { get; init; }

  [JsonPropertyName(ParameterNames.ClientUri)]
  public string ClientUri { get; init; }

  [JsonPropertyName(ParameterNames.BackChannelLogoutUri)]
  public string BackChannelLogoutUri { get; init; }
}