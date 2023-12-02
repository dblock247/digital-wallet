using System.IO;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Passbook.Generator.Tags;
using Xunit;

namespace Passbook.Generator.Tests;

public class SemanticTagsTests
{
    [Fact]
    public void EnsureSemanticFieldsIsGeneratedCorrectly()
    {
        var request = new PassGeneratorRequest();
        request.SemanticTags.Add(new AirlineCode("EX"));
        request.SemanticTags.Add(new Balance("1000", "GBP"));

        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);
        using var writer = new JsonTextWriter(sr);
        writer.Formatting = Formatting.Indented;
        request.Write(writer);
        writer.Flush();

        var jsonString = Encoding.UTF8.GetString(ms.ToArray());
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        var json = (JsonConvert.DeserializeObject<dynamic>(jsonString, settings) as JToken)!;

        var semantics = json["semantics"]!;
        semantics["airlineCode"]!.Value<string>()
            .Should().Be("EX");

        var balance = semantics["balance"]!;
        balance["amount"]!.Value<string>()
            .Should().Be("1000");

        balance["currencyCode"]!.Value<string>()
            .Should().Be("GBP");
    }
}
