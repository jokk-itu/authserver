using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AuthServer.Extensions;

public static class QueryCollectionExtensions
{
    public static string GetValueOrEmpty(this IQueryCollection queryCollection, string key)
    {
        queryCollection.TryGetValue(key, out var value);
        return value == StringValues.Empty ? string.Empty : value.ToString();
    }

    public static IReadOnlyCollection<string> GetSpaceDelimitedValueOrEmpty(this IQueryCollection queryCollection, string key)
    {
        queryCollection.TryGetValue(key, out var value);
        var hasValue = !StringValues.IsNullOrEmpty(value);
        return !hasValue ? [] : value.ToString().Split(' ');
    }
}