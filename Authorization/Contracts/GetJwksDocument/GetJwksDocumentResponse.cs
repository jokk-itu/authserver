using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts.GetJwksDocument;
public class GetJwksDocumentResponse
{
  [JsonPropertyName("keys")]
  public ICollection<JwkDto> Keys { get; set; } = new List<JwkDto>();
}
