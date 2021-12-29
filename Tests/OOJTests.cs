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
            Assert.Equal(outputJson[i], list[i].ToJsonString());
        }
    }

    [Fact]
    public void TestComplexOOJ()
    {
        string[] inputJson =
        {
            "{\"name\":\"base\",\"obj\":{\"value1\":\"abc\",\"obj2\":{\"value2\":\"def\",\"value3\":\"ghi\"}}}",
            "{\"inherit\":\"base\",\"+obj\":{\"+obj2\":{\"value3\":\"jkl\"}}}"
        };

        string[] outputJson =
        {
            "{\"name\":\"base\",\"obj\":{\"value1\":\"abc\",\"obj2\":{\"value2\":\"def\",\"value3\":\"ghi\"}}}",
            "{\"inherit\":\"base\",\"obj\":{\"value1\":\"abc\",\"obj2\":{\"value2\":\"def\",\"value3\":\"jkl\"}}}"
        };

        var list = inputJson.Select(s => (JsonObject)JsonNode.Parse(s)!).ToList();

        OOJson.Solve(list, new OOJsonOptions("name", "inherit"));

        for (int i = 0; i < list.Count; i++)
        {
            Assert.Equal(outputJson[i], list[i].ToJsonString());
        }
    }
}
