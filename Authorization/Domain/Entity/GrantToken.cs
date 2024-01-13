namespace Domain.Entity;
#nullable disable
public abstract class GrantToken : Token
{
    public AuthorizationCodeGrant AuthorizationGrant { get; set; }
}