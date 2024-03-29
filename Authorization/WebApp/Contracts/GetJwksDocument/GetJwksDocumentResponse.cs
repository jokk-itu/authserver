﻿using System.Text.Json.Serialization;

namespace WebApp.Contracts.GetJwksDocument;
public class GetJwksDocumentResponse
{
  [JsonPropertyName("keys")]
  public ICollection<JwkDto> Keys { get; set; } = new List<JwkDto>();
}