namespace Domain;

#nullable disable
public class Jwk
{
  public long KeyId { get; set; }
  public DateTime CreatedTimestamp { get; set; } = DateTime.Now;
  public byte[] PrivateKey { get; set; }
  public byte[] Modulus { get; set; }
  public byte[] Exponent { get; set; }
}