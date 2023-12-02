using Newtonsoft.Json;
using Passbook.Generator.Enums;

namespace Passbook.Generator.Fields;

public class NumberField : Field
{
    public NumberField()
    {
    }

    public NumberField(string key, string label, decimal value, FieldNumberStyle numberStyle)
        : base(key, label)
    {
        Value = value;
        NumberStyle = numberStyle;
    }

    public NumberField(string key, string label, int value, FieldNumberStyle numberStyle)
        : this(key, label, (decimal) value, numberStyle)
    {
    }

    /// <summary>
    /// ISO 4217 currency code for the field’s value.
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// Style of number to display. Must be one of <see cref="FieldNumberStyle" />
    /// </summary>
    public FieldNumberStyle NumberStyle { get; set; }

    public decimal Value { get; set; }

    protected override void WriteKeys(JsonWriter writer)
    {
        if (!string.IsNullOrEmpty(CurrencyCode))
        {
            writer.WritePropertyName("currencyCode");
            writer.WriteValue(CurrencyCode);
        }

        if (NumberStyle == FieldNumberStyle.Unspecified) return;
        writer.WritePropertyName("numberStyle");
        writer.WriteValue(NumberStyle.ToString());
    }

    protected override void WriteValue(JsonWriter writer)
    {
        writer.WriteValue(Value);
    }

    public override void SetValue(object value)
    {
        Value = (decimal) value;
    }

    public override bool HasValue => true;
}
