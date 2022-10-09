namespace Domain;

#nullable disable
public class Session
{
  public long Id { get; set; }
  public User User { get; set; }
  public ICollection<Client> Clients { get; set; }
  public ICollection<AuthorizationCodeGrant> AuthorizationCodeGrants { get; set; }
  public ICollection<IdToken> IdTokens { get; set; }
  public ICollection<AccessToken> AccessTokens { get; set; }
  public ICollection<RefreshToken> RefreshTokens { get; set; }
  public long MaxAge { get; set; }
  public DateTime Created { get; set; }
  public DateTime Updated { get; set; }
}