using System.Text.Json.Nodes;

namespace OOJSON;

public record OOJSolverOptions(string nameProperty="ooj_name", string inheritProperty="ooj_inherit", bool throwOnInvalidOOJ = true);

public class OOJSolver
{
    OOJSolverOptions options;
    List<(string? name, List<string>? inheritance, JsonNode root)> docs = new();

    public OOJSolver(OOJSolverOptions? options=null)
    {
        this.options = options ?? new();
    }

    public void AddRootJSONObject(JsonObject obj)
    {
        if (obj.Root != null) throw new ArgumentException("JsonObject is not root");

        string? name = null;
        List<string>? inheritance = null;
        try
        {
            name = obj[options.nameProperty]?.GetValue<string>();
            inheritance = obj[options.inheritProperty]?.AsArray().Select(x => x!.GetValue<string>()).ToList();
        }
        catch
        {
           if (options.throwOnInvalidOOJ) throw;
        }

        docs.Add((name, inheritance, obj));
    }

    public List<JsonNode> Solve()
    {

    }
}
