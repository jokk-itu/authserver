namespace Application;

#nullable disable
public record IdentityConfiguration
{
  public virtual string Issuer { get; set; }

  public virtual string PrivateKeySecret { get; set; }

  public virtual string EncryptingKeySecret { get; set; }

  public virtual string CodeSecret { get; set; }

  public virtual bool UseReferenceTokens { get; set; }

  /// <summary>
  /// Defined in seconds.
  /// </summary>
  public virtual int AccessTokenExpiration { get; set; }

  /// <summary>
  /// Defined in seconds.
  /// </summary>
  public virtual int RefreshTokenExpiration { get; set; }

  /// <summary>
  /// Defined in seconds.
  /// </summary>
  public virtual int IdTokenExpiration { get; set; }
}