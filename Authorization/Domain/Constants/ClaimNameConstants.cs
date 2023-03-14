namespace Domain.Constants;
public static class ClaimNameConstants
{
  /// <summary>
  /// Scope.
  /// A space delimited string of scopes.
  /// </summary>
  public const string Scope = "scope";

  /// <summary>
  /// Client Id.
  /// </summary>
  public const string ClientId = "client_id";

  /// <summary>
  /// Resource Id.
  /// </summary>
  public const string ResourceId = "resource_id";

  /// <summary>
  /// Scope Id.
  /// </summary>
  public const string ScopeId = "scope_id";

  /// <summary>
  /// Subject Id.
  /// </summary>
  public const string Sub = "sub";

  /// <summary>
  /// JWT ID.
  /// </summary>
  public const string Jti = "jti";

  /// <summary>
  /// Session Id.
  /// </summary>
  public const string Sid = "sid";

  /// <summary>
  /// A number which is only used once.
  /// </summary>
  public const string Nonce = "nonce";

  /// <summary>
  /// Authentication Context Class Reference.
  /// An integer specifying the level of authentication which the End-User used.
  /// Level 0 MUST NOT be accepted.
  /// </summary>
  public const string Acr = "acr";

  /// <summary>
  /// Authentication Methods References.
  /// An array with identifiers for authentication methods.
  /// </summary>
  public const string Amr = "amr";

  /// <summary>
  /// Authorized party.
  /// The party to which the id_token was issued.
  /// </summary>
  public const string Azp = "azp";

  /// <summary>
  /// Expires.
  /// In UNIX seconds
  /// </summary>
  public const string Exp = "exp";

  /// <summary>
  /// Time when the End-User authentication occurred.
  /// </summary>
  public const string AuthTime = "auth_time";

  /// <summary>
  /// Issued At.
  /// In UNIX seconds.
  /// </summary>
  public const string Iat = "iat";

  /// <summary>
  /// Audience.
  /// </summary>
  public const string Aud = "aud";

  /// <summary>
  /// Issuer.
  /// </summary>
  public const string Iss = "iss";

  /// <summary>
  /// Id of the issued grant.
  /// </summary>
  public const string GrantId = "grant_id";

  /// <summary>
  /// Events object.
  /// </summary>
  public const string Events = "events";

  public const string Address = "address";
  public const string GivenName = "given_name";
  public const string FamilyName = "family_name";
  public const string Birthdate = "birthdate";
  public const string Name = "name";
  public const string Email = "email";
  public const string Role = "role";
  public const string Phone = "phone";
  public const string Locale = "locale";
}