using Microsoft.AspNetCore.Identity;

namespace Domain;

public class User : IdentityUser
{
  public string Address { get; set; } = null!;

  public string Name { get; set; } = null!;

  public string FamilyName { get; set; } = null!;

  public string GivenName { get; set; } = null!;

  public string? MiddleName { get; set; }

  public string? NickName { get; set; }

  public string? Gender { get; set; }

  public DateTime Birthdate { get; set; }

  public string Locale { get; set; } = null!;
}