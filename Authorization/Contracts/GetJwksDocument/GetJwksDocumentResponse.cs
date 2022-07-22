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
  public IEnumerable<JwkDto> Keys { get; set; }
}
