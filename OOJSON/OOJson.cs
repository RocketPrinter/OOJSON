using System.Text.Json.Nodes;

namespace OOJSON;

public record OOJsonOptions(string nameProperty="ooj_name", string inheritProperty="ooj_inherit");

public static class OOJson
{
    class Document
    {
        public readonly JsonObject node;
        public readonly string? name;
        public List<JsonObject>? deps;
        public bool visited; 

        public Document(JsonNode node, OOJsonOptions options)
        {
            this.node = node;
            (node[options.nameProperty] as JsonValue)?.TryGetValue(out name);
        }
    }

    /// <summary>
    /// Mutates the nodes in the collection to solve all merges
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static void Solve(ICollection<JsonObject> rootNodes, OOJsonOptions? options = null)
    {
        options ??= new();

        // make sure nodes are roots of trees
        foreach (var node in rootNodes)
        {
            if (node.Root != null) throw new ArgumentException("JsonObject is not root");
        }

        // make dictionary
        var docs = rootNodes.ToDictionary<JsonObject, JsonObject, Document>(node=>node,node=>new(node,options));

        // solve names
        foreach (var kvp in docs)
        {
            // the ugliest linq in existance
            kvp.Value.deps = (kvp.Key[options.inheritProperty] as JsonArray)
                ?.Select(x => { string? name = null; (x as JsonValue)?.TryGetValue(out name); return name; })
                .Where(x => x != null)
                .Select(x => docs.Where(y=>y.Value.name == x).Select(y=>y.Key).FirstOrDefault()! )
                .Where(x => x != null)
                .ToList();
        }

        // topological sorting
        Stack<Document> docStack = new();
        foreach (var kvp in docs)
                TopoSort(kvp.Value);

        void TopoSort(Document doc)
        {
            if (doc.visited) return;
            doc.visited = true;

            foreach (var dep in doc.deps ?? Enumerable.Empty<JsonObject>())
            {
                TopoSort(docs[dep]);
            }

            docStack.Push(doc);
        }

        // solve
        foreach (var doc in docStack)
            foreach (var dep in doc.deps ?? Enumerable.Empty<JsonObject>())
                SolveJsonObject(doc.node, dep); 

        void SolveJsonObject(JsonObject node, JsonObject dep)
        {
            foreach ((string key,var depChild) in dep)
            {
                if (depChild == null) continue;

                // 1) existing node replaces inherited node
                {
                    var nodeChild = node[key];
                    if (nodeChild != null) continue;
                }

                // copy depChild
                var newNode = depChild.Copy();
                node.Add(key, newNode);

                // 2) inherited node but additive
                string pluskey = "+" + key;
                var plusChild = node[pluskey];
                if (plusChild != null)
                {
                    if (plusChild is JsonObject plusChildObj && newNode is JsonObject newNodeObj)
                    {
                        SolveJsonObject(newNodeObj, plusChildObj);
                    }
                    else if (plusChild is JsonArray plusChildArray && newNode is JsonArray newNodeArray)
                    {
                        foreach (var item in plusChildArray)
                        {
                            newNodeArray.Add(item!.Copy());
                        }
                    }

                    node.Remove(pluskey);
                    continue;
                }

                // 3) inherit depChild (already done)
            }
        }
    }
}
