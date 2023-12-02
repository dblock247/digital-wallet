using System.Collections.Generic;
using Newtonsoft.Json;

namespace Passbook.Generator.Tags.Models;

public class SemanticTags : List<SemanticTag>
{
    public void Write(JsonWriter writer)
    {
        writer.WritePropertyName("semantics");
        writer.WriteStartObject();

        foreach (var tag in this)
        {
            tag.Write(writer);
        }

        writer.WriteEndObject();
    }
}
