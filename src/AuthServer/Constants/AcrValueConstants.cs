namespace AuthServer.Constants;

public static class AcrValueConstants
{
    public const string Password = "password";
    public const string TwoFactorAuthentication = "2fa";
    public const string MultiFactorAuthentication = "mfa";

    public static readonly string[] AcrValues = [Password, TwoFactorAuthentication, MultiFactorAuthentication];
}