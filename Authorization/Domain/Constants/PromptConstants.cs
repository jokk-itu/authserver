namespace Domain.Constants;
public static class PromptConstants
{
  public const string Login = "login";
  public const string Consent = "consent";
  public const string None = "none";
  public const string SelectAccount = "select_account";

  public static readonly string[] Prompts =
  {
      $"{Consent} {Login}",
      $"{Login} {Consent}",
      Login,
      None,
      SelectAccount
  };
}