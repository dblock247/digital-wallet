using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Passbook.Generator.Exceptions;
using Passbook.Generator.Fields;
using TimeZoneConverter;
using Xunit;
using Xunit.Abstractions;

namespace Passbook.Generator.Tests;

public class GeneratorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GeneratorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EnsurePassIsGeneratedCorrectly()
    {
        var request = new PassGeneratorRequest
        {
            ExpirationDate = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Local),
            Nfc = new Nfc("My NFC Message", "SKLSJLKJ")
        };

        var offset = new DateTime(2018, 01, 05, 12, 00, 0);
        var zone = TZConvert.GetTimeZoneInfo("Eastern Standard Time");
        var offsetConverted = new DateTimeOffset(offset, zone.GetUtcOffset(offset));

        request.RelevantDate = offsetConverted;

        request.AddAuxiliaryField(new StandardField
        {
            Key = "aux-1",
            Value = "Test",
            Label = "Label",
            Row = 1
        });

        request.AssociatedStoreIdentifiers.Add(long.MaxValue);

        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);
        using var writer = new JsonTextWriter(sr);
        writer.Formatting = Formatting.Indented;
        request.Write(writer);
        writer.Flush();

        var jsonString = Encoding.UTF8.GetString(ms.ToArray());
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        var json = (JsonConvert.DeserializeObject<dynamic>(jsonString, settings) as JToken)!;

        json["relevantDate"]!.ToObject<string>()
            .Should().Be("2018-01-05T12:00:00-05:00");

        json["nfc"]!["message"]!.ToObject<string>()
            .Should().Be("My NFC Message");

        var auxiliaryFields = json["generic"]!["auxiliaryFields"]!.Children<JToken>();
        auxiliaryFields.Should().ContainSingle();

        var auxiliaryField = auxiliaryFields.First();
        auxiliaryField["key"]!.ToObject<string>()
            .Should().Be("aux-1");
        auxiliaryField["value"]!.ToObject<string>()
            .Should().Be("Test");
        auxiliaryField["label"]!.ToObject<string>()
            .Should().Be("Label");
        auxiliaryField["row"]!.ToObject<int>()
            .Should().Be(1);

        var associatedAppIdentifiersPayload = json["associatedStoreIdentifiers"]!.Children();
        associatedAppIdentifiersPayload.Count().Should().Be(1);
        associatedAppIdentifiersPayload.First().ToObject<long>()
            .Should().Be(long.MaxValue);
    }

    [Fact]
    public void EnsureDuplicateKeysThrowAnException()
    {
        var request = new PassGeneratorRequest();
        request.AddAuxiliaryField(new StandardField
        {
            Key = "aux-1",
            Value = "Test",
            Label = "Label",
        });

        request.Invoking(r => r.AddAuxiliaryField(new StandardField
        {
            Key = "aux-1",
            Value = "Test",
            Label = "Label",
        })).Should().Throw<DuplicateFieldKeyException>();
    }

    [Fact]
    public void EnsureFieldHasLocalTime()
    {
        var sut = new PassGeneratorRequest
        {
            ExpirationDate = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Local),
            Nfc = new Nfc("My NFC Message", "SKLSJLKJ")
        };

        var offset = new DateTime(2018, 01, 05, 12, 00, 0);
        var zone = TZConvert.GetTimeZoneInfo("Eastern Standard Time");
        var offsetConverted = new DateTimeOffset(offset, zone.GetUtcOffset(offset));

        sut.RelevantDate = offsetConverted;
        sut.AddAuxiliaryField(new StandardField
        {
            Key = "aux-1",
            Value = "Test",
            Label = "Label",
            Row = 1
        });

        var local = DateTime.Now;
        local = new DateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second, local.Kind);
        sut.AddAuxiliaryField(new DateField
        {
            Key = "datetime-1",
            Value = local,
            Label = "Label",
        });

        var utc = DateTime.UtcNow;
        utc = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Kind);
        sut.AddAuxiliaryField(new DateField
        {
            Key = "datetime-2",
            Value = utc,
            Label = "Label",
        });

        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);
        using var writer = new JsonTextWriter(sr);
        writer.Formatting = Formatting.Indented;
        sut.Write(writer);
        writer.Flush();

        var jsonString = Encoding.UTF8.GetString(ms.ToArray());
        _testOutputHelper.WriteLine(jsonString);
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        var json = (JsonConvert.DeserializeObject<dynamic>(jsonString, settings) as JToken)!;

        json["relevantDate"]!.ToObject<string>()
            .Should().Be("2018-01-05T12:00:00-05:00");

        json["nfc"]!["message"]!.ToObject<string>()
            .Should().Be("My NFC Message");

        var auxiliaryFields = json["generic"]!["auxiliaryFields"]!.ToArray();
        // auxiliaryFields.Should().HaveCount(3);

        auxiliaryFields.Should().HaveCount(3)
            .And.SatisfyRespectively(
            first =>
            {
                first["key"]!.ToObject<string>()
                    .Should().Be("aux-1");
                first["value"]!.ToObject<string>()
                    .Should().Be("Test");
                first["label"]!.ToObject<string>()
                    .Should().Be("Label");
                first["row"]!.ToObject<int>()
                    .Should().Be(1);
            },
            second =>
            {
                second["key"]!.ToObject<string>()
                    .Should().Be("datetime-1");
                second["value"]!.ToObject<string>()
                    .Should().StartWith($"{local:yyyy-MM-ddTHH:mm}")
                    .And.NotContain("Z");
                second["label"]!.ToObject<string>()
                    .Should().Be("Label");
            },
            third =>
            {
                third["key"]!.ToObject<string>()
                    .Should().Be("datetime-2");
                third["value"]!.ToObject<string>()
                    .Should().Be($"{utc:yyyy-MM-ddTHH:mm:ss}Z")
                    .And.Contain("Z");
                third["label"]!.ToObject<string>()
                    .Should().Be("Label");
            });

        // var auxiliaryField = auxiliaryFields[0];
        // auxiliaryField["key"]!.ToObject<string>()
        //     .Should().Be("aux-1");
        // auxiliaryField["value"]!.ToObject<string>()
        //     .Should().Be("Test");
        // auxiliaryField["label"]!.ToObject<string>()
        //     .Should().Be("Label");
        // auxiliaryField["row"]!.ToObject<int>()
        //     .Should().Be(1);
        //
        // var datetimeField = auxiliaryFields[1];
        // datetimeField["key"]!.ToObject<string>()
        //     .Should().Be("datetime-1");
        // datetimeField["value"]!.ToObject<string>()
        //     .Should().StartWith($"{local:yyyy-MM-ddTHH:mm}")
        //     .And.NotContain("Z");
        // datetimeField["label"]!.ToObject<string>()
        //     .Should().Be("Label");
        //
        // var utcDatetimeField = auxiliaryFields[2];
        // utcDatetimeField["key"]!.ToObject<string>()
        //     .Should().Be("datetime-2");
        // utcDatetimeField["value"]!.ToObject<string>()
        //     .Should().Be($"{utc:yyyy-MM-ddTHH:mm:ss}Z")
        //     .And.Contain("Z");
        // utcDatetimeField["label"]!.ToObject<string>()
        //     .Should().Be("Label");
    }
}
