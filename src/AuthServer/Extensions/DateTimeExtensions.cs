namespace AuthServer.Extensions;
internal static class DateTimeExtensions
{
    public static DateTime ToDateTimeUtc(this long unixTimeSeconds)
    {
        return DateTime.UnixEpoch.AddSeconds(unixTimeSeconds);
    }

    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return (long)dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}
