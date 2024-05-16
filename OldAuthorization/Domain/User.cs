namespace Domain;

#nullable disable
public class User
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string UserName { get; set; }
  public string Password { get; set; }
  public string PhoneNumber { get; set; }
  public string Email { get; set; }
  public string Address { get; set; }
  public string LastName { get; set; }
  public string FirstName { get; set; }
  public DateTime Birthdate { get; set; }
  public string Locale { get; set; }
  public ICollection<ConsentGrant> ConsentGrants { get; set; } = new List<ConsentGrant>();
  public ICollection<Session> Sessions { get; set; } = new List<Session>();
  public ICollection<Role> Roles { get; set; } = new List<Role>();
  public ICollection<PairwiseIdentifier> PairwiseIdentifiers { get; set; } = new List<PairwiseIdentifier>();

  public string GetName()
  {
    return $"{FirstName} {LastName}";
  }
}