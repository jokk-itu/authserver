namespace AuthServer.Constants;

/// <summary>
/// Values defined at IANA.
/// <remarks>https://www.iana.org/assignments/authentication-method-reference-values/authentication-method-reference-values.xhtml</remarks>
/// </summary>
public static class AuthenticationMethodReferenceConstants
{
    public const string Face = "face";
    public const string Fingerprint = "fpt";
    public const string Geo = "geo";
    public const string ProofOfPossessionHardwareKey = "hwk";
    public const string Iris = "iris";
    public const string KnowledgeBasedAuthentication = "kba";
    public const string MultipleChannelAuthentication = "mca";
    public const string MultiFactorAuthentication = "mfa";
    public const string OneTimePassword = "otp";
    public const string PersonalIdentificationNumber = "pin";
    public const string ProofOfPossessionKey = "pop";
    public const string Password = "pwd";
    public const string RiskBasedAuthentication = "rba";
    public const string Retina = "retina";
    public const string SmartCard = "sc";
    public const string Sms = "sms";
    public const string ProofOfPossessionSoftwareKey = "swk";
    public const string TelephoneCall = "tel";
    public const string User = "user";
    public const string Voice = "vbm";
    public const string WindowsIntegratedAuthentication = "wia";
}