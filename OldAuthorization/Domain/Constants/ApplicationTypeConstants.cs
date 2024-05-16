namespace Domain.Constants;
public static class ApplicationTypeConstants
{
  public const string Web = "web";
  public const string Native = "native";

  public static readonly ICollection<string> ApplicationTypes = new[] { Web, Native };
}
