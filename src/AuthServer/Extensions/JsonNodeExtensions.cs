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
        return node[key]?
                   .AsArray()
                   .Select(x => x?.GetValue<string>() ?? string.Empty)
                   .ToList()
                   .AsReadOnly()
               ?? new List<string>().AsReadOnly();
    }

    public static IReadOnlyCollection<string> GetSpaceDelimitedValue(this JsonNode node, string key)
    {
        return node[key]?.GetValue<string>()?.Split(' ') ?? [];
    }
}