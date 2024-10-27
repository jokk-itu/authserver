namespace AuthServer.Helpers;

internal static class MaxAgeHelper
{
    public static bool IsMaxAgeValid(string? maxAge)
    {
        return string.IsNullOrWhiteSpace(maxAge)
               || int.TryParse(maxAge, out var parsedMaxAge)
               && parsedMaxAge > -1;
    }
}