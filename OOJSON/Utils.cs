using System.Text.Json.Nodes;

namespace OOJSON;

internal static class Utils
{
    internal static JsonNode Copy(this JsonNode node) => JsonNode.Parse(node.ToJsonString())!;
}
