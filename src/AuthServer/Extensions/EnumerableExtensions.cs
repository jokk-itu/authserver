namespace AuthServer.Extensions;
internal static class EnumerableExtensions
{
    /// <summary>
    /// Source is the suspicious input, and ValidInput is the source of truth.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="validInput"></param>
    /// <returns>false if source contains any input that is not found in validInput or if source is empty</returns>
    public static bool ExceptAny<T>(this IEnumerable<T> source, IEnumerable<T> validInput) =>
        source.Except(validInput).Any();
}
