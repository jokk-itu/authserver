namespace Domain;

#nullable disable
public class Nonce
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Value { get; set; }
  public DateTime IssuedAt { get; set; }
  public AuthorizationCodeGrant AuthorizationCodeGrant { get; set; }
}