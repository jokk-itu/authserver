namespace AuthServer.Constants;
public static class ApplicationTypeConstants
{
    public const string Web = "web";
    public const string Native = "native";

    public static readonly IReadOnlyCollection<string> ApplicationTypes = [Web, Native];
}
