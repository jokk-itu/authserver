namespace AuthServer.Extensions;
internal static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return (long)dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}
