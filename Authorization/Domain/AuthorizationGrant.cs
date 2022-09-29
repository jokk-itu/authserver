namespace Domain;

#nullable disable
public class AuthorizationGrant : Grant
{
  public User User { get; set; }

  public Client Client { get; set; }

  public RedirectUri RedirectUri { get; set; }

  public string Nonce { get; set; }

  public string CodeChallenge { get; set; }

  public GrantType GrantType { get; set; }
}