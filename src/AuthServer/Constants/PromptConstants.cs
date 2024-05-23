namespace AuthServer.Constants;

public static class PromptConstants
{
    public const string Login = "login";
    public const string Consent = "consent";
    public const string None = "none";
    public const string SelectAccount = "select_account";
    public const string Create = "create";

    public static readonly string[] Prompts =
        [Login, None, SelectAccount, Consent, Create];
}