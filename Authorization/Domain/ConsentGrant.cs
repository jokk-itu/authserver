namespace Domain;

#nullable disable
public class ConsentGrant
{
  public int Id { get; set; }
  public DateTime Updated { get; set; }
  public Client Client { get; set; }
  public User User { get; set; }
  public ICollection<Claim> ConsentedClaims { get; set; } = new List<Claim>();
  public ICollection<Scope> ConsentedScopes { get; set; } = new List<Scope>();
}