using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostScope;

#nullable disable
public class PostScopeRequest
{
  [JsonPropertyName(ParameterNames.ScopeName)]
  public string ScopeName { get; init; }
}