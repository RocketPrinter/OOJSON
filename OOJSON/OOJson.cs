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

        public Document(JsonObject node, OOJsonOptions options)
        {
            this.node = node;
            name = (node[options.nameProperty] as JsonValue)?.GetRawValue();
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
            if (node.Root != null && node.Root != node) 
                throw new ArgumentException("JsonObject is not root");
        }

        // make dictionary
        var docs = rootNodes.ToDictionary<JsonObject, JsonObject, Document>(node=>node,node=>new(node,options));

        // solve names
        foreach (var kvp in docs)
        {
            // the ugliest linq in existance
            kvp.Value.deps = (kvp.Key[options.inheritProperty] switch
            {
                JsonArray arr => arr.Select(x => (x as JsonValue)?.GetRawValue()).Where(x=>x!=null),
                JsonValue val => new[] { val.GetRawValue() },
                _ => Enumerable.Empty<string>()
            })
            .Select(x => docs.Where(y => y.Value.name == x).Select(y => y.Key).FirstOrDefault()!)
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
        {
            foreach (var dep in doc.deps ?? Enumerable.Empty<JsonObject>())
            {
                foreach ((string key, JsonNode? depChild) in dep)
                {
                    // we ignore those
                    if (key == options.nameProperty || key == options.inheritProperty)
                        continue;

                    // existing node replaces inherited node
                    if (doc.node[key] != null)
                        continue;

                    // copy inherited node
                    var newNode = depChild!.Copy();
                    doc.node.Add(key, newNode);

                    // if there's a node with +key we merge it
                    var plusNode = doc.node["+" + key];
                    if (plusNode != null)
                    {
                        RecursiveMerge(newNode, plusNode);

                        // delete +key node
                        doc.node.Remove("+" + key);
                    }
                }

            }
        }

        void RecursiveMerge(JsonNode node, JsonNode plusNode)
        {
            if (node.GetType() != plusNode.GetType()) // missmatching type)
                return;

            switch (node)
            {
                case JsonObject nodeObj:
                    var plusObj = (JsonObject)plusNode;
                    foreach ((string key, JsonNode? plusChild) in plusObj)
                    {
                        if (key.Length == 0) continue;

                        var child = nodeObj[key[1..]];

                       if (child != null)
                        {
                            RecursiveMerge(child!, plusChild!);
                            continue;
                        }

                        child = nodeObj[key];

                        if (child != null) 
                            nodeObj.Remove(key);

                        nodeObj.Add(key, plusChild!.Copy());
                    }
                    break;

                case JsonArray nodeArr:
                    var plusArr = (JsonArray)plusNode;
                    foreach (var child in plusArr)
                        nodeArr.Add(child!.Copy());
                    break;
             
                default:
                    break;
            }
        }
    }
}
