using System.Text.Json.Nodes;

namespace AuthServer.Extensions;

internal static class JsonNodeExtensions
{
	public static string GetValueOrEmpty(this JsonNode node, string key)
	{
		return node[key]?.GetValue<string>() ?? string.Empty;
	}

	public static IReadOnlyCollection<string> GetSpaceDelimitedValueOrEmpty(this JsonNode node, string key)
	{
		return node[key]?.GetValue<string[]>() ?? [];
	}
}