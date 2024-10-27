using System.Text.Json.Nodes;

namespace AuthServer.Extensions;

internal static class JsonNodeExtensions
{
	public static string? GetValue(this JsonNode node, string key)
	{
		return node[key]?.GetValue<string?>();
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
        return node[key]?.GetValue<string>()?.Split(' ') ?? [];
    }
}