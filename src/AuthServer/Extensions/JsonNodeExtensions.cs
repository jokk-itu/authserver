using System.Text.Json.Nodes;

namespace AuthServer.Extensions;

internal static class JsonNodeExtensions
{
    public static string? GetStringValue(this JsonNode node, string key)
    {
        try
        {
            return node[key]?.GetValue<string?>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static bool? GetBoolValue(this JsonNode node, string key)
    {
        try
        {
            return node[key]?.GetValue<bool?>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static int? GetIntValue(this JsonNode node, string key)
    {
        try
        {
            return node[key]?.GetValue<int?>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static IReadOnlyCollection<string> GetCollectionValue(this JsonNode node, string key)
    {
        var jsonNode = node[key];
        if (jsonNode is JsonArray jsonArray)
        {
            return jsonArray
                .Select(x => x?.GetValue<string>())
                .OfType<string>()
                .ToList()
                .AsReadOnly();
        }

        return [];
    }

    public static IReadOnlyCollection<string> GetSpaceDelimitedValue(this JsonNode node, string key)
    {
        try
        {
            return node[key]?.GetValue<string?>()?.Split(' ') ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }
}