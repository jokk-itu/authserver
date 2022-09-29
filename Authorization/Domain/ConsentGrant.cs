namespace Domain;

#nullable disable
public class ConsentGrant : Grant
{
  public Client Client { get; set; }

  public User User { get; set; }

  public ICollection<Claim> ConsentedClaims { get; set; }
}