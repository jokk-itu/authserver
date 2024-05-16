namespace Domain;

#nullable disable
public class Jwk
{
  public long Id { get; set; }
  public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
  public byte[] PrivateKey { get; set; }
  public byte[] Modulus { get; set; }
  public byte[] Exponent { get; set; }
}