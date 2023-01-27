namespace Domain.Constants;
public static class PromptConstants
{
  public const string Login = "login";
  public const string Consent = "consent";
  public const string Create = "create";

  public static readonly string[] Prompts = { $"{Consent} {Login}" ,$"{Login} {Consent}", Create, Login };
}