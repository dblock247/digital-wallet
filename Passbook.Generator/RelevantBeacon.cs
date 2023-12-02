using Newtonsoft.Json;
using Passbook.Generator.Exceptions;

namespace Passbook.Generator;

public class RelevantBeacon
{
    /// <summary>
    /// Required. Unique identifier of a Bluetooth Low Energy location beacon.
    /// </summary>
    public string ProximityUuid { get; set; } = null!;

    /// <summary>
    /// Optional. Text displayed on the lock screen when the pass is currently relevant.
    /// </summary>
    public string RelevantText { get; set; } = null!;

    public int? Major { get; set; } = null!;

    public int? Minor { get; set; } = null!;

    public void Write(JsonWriter writer)
    {
        Validate();

        writer.WriteStartObject();

        writer.WritePropertyName("proximityUUID");
        writer.WriteValue(ProximityUuid);

        if (!string.IsNullOrEmpty(RelevantText))
        {
            writer.WritePropertyName("relevantText");
            writer.WriteValue(RelevantText);
        }

        if (Minor.HasValue)
        {
            writer.WritePropertyName("minor");
            writer.WriteValue(Minor);
        }

        if (Major.HasValue)
        {
            writer.WritePropertyName("major");
            writer.WriteValue(Major);
        }

        writer.WriteEndObject();
    }

    private void Validate()
    {
        if (string.IsNullOrEmpty(ProximityUuid))
        {
            throw new RequiredFieldValueMissingException("ProximityUuid");
        }
    }
}
