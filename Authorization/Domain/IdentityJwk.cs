namespace Domain;

public class IdentityJwk
{
  public long KeyId { get; set; }
  public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.Now;
  public byte[] PrivateKey { get; set; }
  public byte[] Modulus { get; set; }
  public byte[] Exponent { get; set; }
}