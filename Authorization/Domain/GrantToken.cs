namespace Domain;
#nullable disable
public abstract class GrantToken : Token
{
  public AuthorizationCodeGrant AuthorizationGrant { get; set; }
}