using Microsoft.AspNetCore.Identity;

namespace Domain;

#nullable enable
public class User : IdentityUser
{
  public string Address { get; set; } = null!;
  public string LastName { get; set; } = null!;
  public string FirstName { get; set; } = null!;
  public DateTime Birthdate { get; set; }
  public string Locale { get; set; } = null!;
  public ICollection<ConsentGrant> ConsentGrants { get; set; } = new List<ConsentGrant>();
  public Session? Session { get; set; }
  public long? SessionId { get; set; }

  public string GetName()
  {
    return $"{FirstName} {LastName}";
  }
}