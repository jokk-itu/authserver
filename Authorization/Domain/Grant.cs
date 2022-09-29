namespace Domain;

#nullable disable
public class Grant
{
  public Guid Id { get; set; }

  public DateTime IssuedAt { get; set; }

  public ICollection<Scope> Scopes { get; set; }

  public bool IsRevoked { get; set; }

  public bool IsRedeemed { get; set; }
}