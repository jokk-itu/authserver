using System.Text.Json.Serialization;

namespace Contracts.GetTokenIntrospeciton
{
    public class GetTokenIntrospectionResponse
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("sub")]
        public string? SubjectId { get; set; }

        [JsonPropertyName("aud")]
        public string Audience { get; set; }

        [JsonPropertyName("iss")]
        public string Issuer { get; set; }

        [JsonPropertyName("exp")]
        public long Expires { get; set; }

        [JsonPropertyName("iat")]
        public long IssuedAt { get; set; }
    }
}