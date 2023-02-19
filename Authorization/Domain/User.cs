namespace Domain;

#nullable enable
public class User
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string UserName { get; set; } = null!;
  public string Password { get; set; } = null!;
  public string PhoneNumber { get; set; } = null!;
  public string Email { get; set; } = null!;
  public bool IsEmailVerified { get; set; }
  public bool IsPhoneNumberVerified { get; set; }
  public string Address { get; set; } = null!;
  public string LastName { get; set; } = null!;
  public string FirstName { get; set; } = null!;
  public DateTime Birthdate { get; set; }
  public string Locale { get; set; } = null!;
  public ICollection<ConsentGrant> ConsentGrants { get; set; } = new List<ConsentGrant>();
  public ICollection<Session> Sessions { get; set; } = new List<Session>();

  public string GetName()
  {
    return $"{FirstName} {LastName}";
  }
}