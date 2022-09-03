namespace Infrastructure;

#nullable disable
public record IdentityConfiguration
{
  public virtual string Audience { get; init; }

  public virtual string ExternalIssuer { get; init; }

  public virtual string InternalIssuer { get; init; }

  public virtual string PrivateKeySecret { get; init; }

  public virtual string CodeSecret { get; init; }

  //Seconds
  public virtual int AccessTokenExpiration { get; init; }

  //Seconds
  public virtual int RefreshTokenExpiration { get; init; }

  //Seconds
  public virtual int IdTokenExpiration { get; init; }

  //Seconds
  public virtual int CodeExpiration { get; init; }
}