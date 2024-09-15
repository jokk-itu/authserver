namespace AuthServer.Constants;

/// <summary>
/// Constants defined at IANA.
/// <remarks>https://www.iana.org/assignments/jwt/jwt.xhtml</remarks>
/// </summary>
public static class ClaimNameConstants
{
    /// <summary>
    /// Scope.
    /// A space delimited string of scopes.
    /// </summary>
    public const string Scope = "scope";

    /// <summary>
    /// Subject ID.
    /// </summary>
    public const string Sub = "sub";

    /// <summary>
    /// JWT ID.
    /// </summary>
    public const string Jti = "jti";

    /// <summary>
    /// Session ID.
    /// </summary>
    public const string Sid = "sid";

    /// <summary>
    /// A value which is only used once.
    /// </summary>
    public const string Nonce = "nonce";

    /// <summary>
    /// Authentication Context Class Reference.
    /// Values are defined at discovery.
    /// </summary>
    public const string Acr = "acr";

    /// <summary>
    /// Authentication Methods References.
    /// Values are defined at IANA.
    /// <remarks>https://www.iana.org/assignments/authentication-method-reference-values/authentication-method-reference-values.xhtml</remarks>
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
    /// JsonWebKey.
    /// </summary>
    public const string Jwk = "jwk";

    /// <summary>
    /// Http method.
    /// </summary>
    public const string Htm = "htm";

    /// <summary>
    /// Http target uri.
    /// </summary>
    public const string Htu = "htu";

    /// <summary>
    /// Hash of an access token.
    /// </summary>
    public const string Ath = "ath";

    /// <summary>
    /// ID of the issued grant.
    /// </summary>
    public const string GrantId = "grant_id";

    /// <summary>
    /// Events object.
    /// </summary>
    public const string Events = "events";

    /// <summary>
    /// Full name
    /// </summary>
    public const string Name = "name";

    /// <summary>
    /// Given name(s) or first name(s)
    /// </summary>
    public const string GivenName = "given_name";

    /// <summary>
    /// Surname(s) or last name(s)
    /// </summary>
    public const string FamilyName = "family_name";

    /// <summary>
    /// Middle name(s)
    /// </summary>
    public const string MiddleName = "middle_name";

    /// <summary>
    /// Casual name
    /// </summary>
    public const string NickName = "nickname";

    /// <summary>
    /// Shorthand name by which the End-User wishes to be referred to
    /// </summary>
    public const string PreferredUsername = "preferred_username";

    /// <summary>
    /// Profile page URL
    /// </summary>
    public const string Profile = "profile";

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public const string Picture = "picture";

    /// <summary>
    /// Web page or blog URL
    /// </summary>
    public const string Website = "website";

    /// <summary>
    /// Preferred e-mail address
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// True if the e-mail address has been verified; otherwise false
    /// </summary>
    public const string EmailVerified = "email_verified";

    /// <summary>
    /// Gender
    /// </summary>
    public const string Gender = "gender";

    /// <summary>
    /// Birthday
    /// </summary>
    public const string Birthdate = "birthdate";

    /// <summary>
    /// Time zone
    /// </summary>
    public const string ZoneInfo = "zoneinfo";

    /// <summary>
    /// Locale
    /// </summary>
    public const string Locale = "locale";

    /// <summary>
    /// Preferred telephone number
    /// </summary>
    public const string PhoneNumber = "phone_number";

    /// <summary>
    /// True if the phone number has been verified; otherwise false
    /// </summary>
    public const string PhoneNumberVerified = "phone_number_verified";

    /// <summary>
    /// Preferred postal address
    /// </summary>
    public const string Address = "address";

    /// <summary>
    /// Time the information was last updated
    /// </summary>
    public const string UpdatedAt = "updated_at";

    /// <summary>
    /// Client Identifier
    /// </summary>
    public const string ClientId = "client_id";

    /// <summary>
    /// Roles
    /// </summary>
    public const string Roles = "roles";

    public static readonly string[] SupportedEndUserClaims =
    [
        Name, GivenName, FamilyName, MiddleName,
        Address, NickName, PreferredUsername,
        Profile, Picture, Website,
        Email, EmailVerified, Gender,
        Birthdate, ZoneInfo, Locale,
        PhoneNumber, PhoneNumberVerified,
        UpdatedAt, Roles
    ];
}