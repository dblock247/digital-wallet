using Newtonsoft.Json;
using Passbook.Generator.Enums;
using Passbook.Generator.Exceptions;

namespace Passbook.Generator.Fields;

public class StandardField : Field
{
    public StandardField()
    {
    }

    public StandardField(string key, string label, string value)
        : base(key, label)
    {
        Value = value;
    }

    public StandardField(string key, string label, string value, string attributedValue,
        DataDetectorTypes dataDetectorTypes)
        : this(key, label, value)
    {
        AttributedValue = attributedValue;
        DataDetectorTypes = dataDetectorTypes;
    }

    public string Value { get; set; }

    protected override void WriteValue(JsonWriter writer)
    {
        if (Value == null)
        {
            throw new RequiredFieldValueMissingException(Key);
        }

        writer.WriteValue(Value);
    }

    public override void SetValue(object value)
    {
        Value = value as string;
    }

    public override bool HasValue => !string.IsNullOrEmpty(Value);
}
