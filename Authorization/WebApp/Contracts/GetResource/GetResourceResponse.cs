using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.GetResource;

#nullable disable
public class GetResourceResponse
{
  [JsonPropertyName(ParameterNames.ResourceId)]
  public string ResourceId { get; init; }

  [JsonPropertyName(ParameterNames.ResourceName)]
  public string ResourceName { get; init; }

  [JsonPropertyName(ParameterNames.ResourceSecret)]
  public string ResourceSecret { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; }
}