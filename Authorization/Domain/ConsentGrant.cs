namespace Domain;

#nullable disable
public class ConsentGrant
{
  public long Id { get; set; }
  public DateTime IssuedAt { get; set; }
  public DateTime Updated { get; set; }
  public bool IsRevoked { get; set; }
  public Client Client { get; set; }
  public User User { get; set; }
  public ICollection<Claim> ConsentedClaims { get; set; }
  public ICollection<Scope> Scopes { get; set; }
}