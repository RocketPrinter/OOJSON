using System.Text.Json.Nodes;

namespace OOJSON;

public record OOJSolverOptions(string nameProperty="ooj_name", string inheritProperty="ooj_inherit", bool throwOnInvalidOOJ = true);

public class OOJSolver
{
    record Document(string? name, List<string>? dependencies, JsonNode root);

    OOJSolverOptions options;
    List<Document> docs = new();

    public OOJSolver(OOJSolverOptions? options=null)
    {
        this.options = options ?? new();
    }

    /// <summary>
    /// Add a JSONObject that is the root of a document to the solver.
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="ArgumentException"></exception>
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

        docs.Add(new(name, inheritance, obj));
    }

    public IEnumerable<JsonNode> Solve()
    {
        // solve strings in dependencies

        // topological sorting
        Stack<Document> docStack = new();
        HashSet<Document> visited = new();
        foreach (var doc in docs)
                TopoSort(doc);

        void TopoSort(Document doc)
        {
            if (!visited.Contains(doc)) return;
            visited.Add(doc);

            foreach (string name in doc.dependencies ?? Enumerable.Empty<string>())
            {
                var child = docs.Find(x => x.name == name);
                if (child != null)
                    TopoSort(child);    
                
                if (options.throwOnInvalidOOJ)
                    throw new Exception($"Cannot find OOJ with name {name}");
            }

            docStack.Push(doc);
        }

        // solve
        List<JsonNode> result = new();
        foreach (var doc in docStack)
        {
            result.Add(doc.root);

        }

        void Apply(JsonNode node, JsonNode baseNode)
        {

        }
        return result;
    }
}
