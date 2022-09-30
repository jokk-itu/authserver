namespace Domain;

#nullable disable
public class Grant
{
  public Guid Id { get; set; }

  public DateTime IssuedAt { get; set; }

  public DateTime Updated { get; set; }

  public long MaxAge { get; set; }

  public ICollection<Scope> Scopes { get; set; }

  public bool IsRevoked { get; set; }

  public bool IsRedeemed { get; set; }

  public bool IsMaxAgeExceeded()
  {
    if (MaxAge == 0)
      return false;

    return Updated.AddSeconds(MaxAge) >= DateTime.Now;
  }
}