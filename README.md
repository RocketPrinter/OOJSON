# OOJSON
### Object oriented JSON. For configuration files mainly.
Don't look at the code. It's very ugly.
I made it to learn how xUnit and deploying nuget packages works and for using on another project

### How2use
Get the [Nuget package](https://www.nuget.org/packages/OOJSON/)

Pass a collection of root nodes of JSON documents to `OOJson.Solve`. The root nodes and their children will be mutated.

### Examples
```json
{
  "name": "base",
  "obj": {
    "value1": "abc",
    "obj2": {
      "value2": "def",
      "value3": "ghi"
      "arr" : ["1"]
    }
  }
}
```
```json
{
  "inherit": "base",
  "+obj": {
    "+obj2": {
      "value3": "jkl"
      "+arr" : ["2"]
    }
  }
}
```
After solving the second document becomes:
```json
{
  "inherit": "base",
  "obj": {
    "value1": "abc",
    "obj2": {
      "value2": "def"
      "value3": "jkl"
      "arr" : ["1","2"]
    }
  }
}
```