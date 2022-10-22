namespace Infrastructure;

#nullable disable
public record IdentityConfiguration
{
  public virtual string ExternalIssuer { get; init; }

  public virtual string InternalIssuer { get; init; }

  public virtual string PrivateKeySecret { get; init; }

  public virtual string CodeSecret { get; init; }

  /// <summary>
  /// Defined in seconds
  /// </summary>
  public virtual int AccessTokenExpiration { get; init; }

  /// <summary>
  /// Defined in seconds
  /// </summary>
  public virtual int RefreshTokenExpiration { get; init; }

  /// <summary>
  /// Defined in seconds
  /// </summary>
  public virtual int IdTokenExpiration { get; init; }

  /// <summary>
  /// Defined in seconds
  /// </summary>
  public virtual int CodeExpiration { get; init; }
}