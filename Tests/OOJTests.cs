using Xunit;
using OOJSON;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Tests;

public class OOJTests
{
    [Fact]
    public void TestValidOOJ()
    {
        string[] inputJson = 
        { 
            "{\"name\":\"ab\",\"object\":{\"a\":\"a\",\"b\":\"b\"},\"array\":[\"a\",\"b\"]}",
            "{\"name\":\"cd\",\"inherit\":[\"ab\"],\"object\":{\"c\":\"c\",\"d\":\"d\"},\"array\":[\"c\",\"d\"]}",
            "{\"name\":\"pluscd\",\"inherit\":[\"ab\"],\"+object\":{\"c\":\"c\",\"d\":\"d\"},\"+array\":[\"c\",\"d\"]}"
        };

        string[] outputJson =
        {
            "{\"name\":\"ab\",\"object\":{\"a\":\"a\",\"b\":\"b\"},\"array\":[\"a\",\"b\"]}",
            "{\"name\":\"cd\",\"inherit\":[\"ab\"],\"object\":{\"c\":\"c\",\"d\":\"d\"},\"array\":[\"c\",\"d\"]}",
            "{\"name\":\"pluscd\",\"inherit\":[\"ab\"],\"object\":{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\",\"d\":\"d\"},\"array\":[\"a\",\"b\",\"c\",\"d\"]}"
        };

        var list = inputJson.Select(s => (JsonObject)JsonNode.Parse(s)!).ToList();

        OOJson.Solve(list, new OOJsonOptions("name", "inherit"));

        for (int i=0;i<list.Count;i++)
        {
            string newJson = list[i].ToJsonString();
            Assert.True(newJson == outputJson[i], $"{newJson}        {outputJson}");
        }
    }

    [Fact]
    public void TestInvalidOOJ()
    {

    }
}
