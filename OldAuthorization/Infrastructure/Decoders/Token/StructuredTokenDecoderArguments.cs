namespace Infrastructure.Decoders.Token;
#nullable disable
public class StructuredTokenDecoderArguments
{
  public bool ValidateLifetime { get; set; }
  public bool ValidateAudience { get; set; }
  public string ClientId { get; set; }
  public IEnumerable<string> Audiences { get; set; } = new List<string>();
}