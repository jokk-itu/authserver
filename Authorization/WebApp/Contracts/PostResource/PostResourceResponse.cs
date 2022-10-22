using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostResource;

#nullable disable
public class PostResourceResponse
{
  [JsonPropertyName(ParameterNames.ResourceId)]
  public string ResourceId { get; set; }

  [JsonPropertyName(ParameterNames.ResourceName)]
  public string ResourceName { get; set; }

  [JsonPropertyName(ParameterNames.ResourceSecret)]
  public string ResourceSecret { get; set; }

  [JsonPropertyName(ParameterNames.RegistrationAccessToken)]
  public string ResourceRegistrationAccessToken { get; set; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; set; }

  [JsonPropertyName(ParameterNames.RegistrationResourceUri)]
  public string RegistrationResourceUri { get; set; }
}
