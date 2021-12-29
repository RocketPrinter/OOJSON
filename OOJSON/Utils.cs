using System.Text.Json.Nodes;

namespace OOJSON;

internal static class Utils
{
    internal static JsonNode Copy(this JsonNode node) => JsonNode.Parse(node.ToJsonString())!;

    internal static string GetRawValue(this JsonValue val)
    {
        val.TryGetValue(out string? s);
        return s!; // a JsonValue should always be able to be converted to a string right?
    }
}
