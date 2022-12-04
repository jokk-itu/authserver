namespace Application;

#nullable disable
public record IdentityConfiguration
{
  public virtual string Issuer { get; init; }

  public virtual string PrivateKeySecret { get; init; }

  public virtual string CodeSecret { get; init; }

  public virtual string EncryptingKeySecret { get; init; }

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
}