using Microsoft.AspNetCore.Identity;

namespace Domain;
public class IdentityUserExtended : IdentityUser
{
  public string Address { get; set; }

  public string Name { get; set; }

  public string FamilyName { get; set; }

  public string GivenName { get; set; }

  public string? MiddleName { get; set; }

  public string? NickName { get; set; }

  public string Gender { get; set; }

  public DateTime Birthdate { get; set; }

  public string Locale { get; set; }
}