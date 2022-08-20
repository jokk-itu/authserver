namespace Infrastructure;

public record IdentityConfiguration
{
  public string Audience { get; init; }

  public string ExternalIssuer { get; init; }

  public string InternalIssuer { get; init; }

  public string PrivateKeySecret { get; init; }

  public string CodeSecret { get; init; }

  //Seconds
  public int AccessTokenExpiration { get; init; }

  //Seconds
  public int RefreshTokenExpiration { get; init; }

  //Seconds
  public int IdTokenExpiration { get; init; }

  //Seconds
  public int CodeExpiration { get; init; }
}