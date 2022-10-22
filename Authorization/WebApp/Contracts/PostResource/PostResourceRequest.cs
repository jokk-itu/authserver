using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostResource;

#nullable disable
public class PostResourceRequest
{
  [JsonPropertyName(ParameterNames.ResourceName)]
  public string ResourceName { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; }
}
