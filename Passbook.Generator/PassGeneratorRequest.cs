using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Passbook.Generator.Enums;
using Passbook.Generator.Exceptions;
using Passbook.Generator.Fields;
using Passbook.Generator.Tags.Models;

namespace Passbook.Generator;

public class PassGeneratorRequest
{
    #region Standard Keys

    /// <summary>
    /// Required. Pass type identifier, as issued by Apple. The value must correspond with your signing certificate.
    /// </summary>
    public string PassTypeIdentifier { get; set; } = null!;

    /// <summary>
    /// Required. Version of the file format. The value must be 1.
    /// </summary>
    public int FormatVersion => 1;

    /// <summary>
    /// Required. Serial number that uniquely identifies the pass. No two passes with the same pass type identifier may have the same serial number.
    /// </summary>
    public string SerialNumber { get; set; } = null!;

    /// <summary>
    /// Required. A simple description of the pass
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Required. Team identifier of the organization that originated and signed the pass, as issued by Apple.
    /// </summary>
    public string TeamIdentifier { get; set; } = null!;

    /// <summary>
    /// Required. Display name of the organization that originated and signed the pass.
    /// </summary>
    public string OrganizationName { get; set; } = null!;

    /// <summary>
    /// Disables sharing of the pass.
    /// </summary>
    public bool SharingProhibited { get; set; }

    #endregion

    #region Image Files

    /// <summary>
    /// When using in memory, the binary of each image is put here.
    /// </summary>
    public Dictionary<PassbookImage, byte[]> Images { get; set; } = new();

    #endregion

    #region Companion App Keys

    #endregion

    #region Expiration Keys

    public DateTimeOffset? ExpirationDate { get; set; } = null!;

    public bool? Voided { get; set; } = null!;

    #endregion

    #region Visual Appearance Keys

    /// <summary>
    /// Optional. Foreground color of the pass, specified as a CSS-style RGB triple. For example, rgb(100, 10, 110).
    /// </summary>
    public string ForegroundColor { get; set; } = null!;

    /// <summary>
    /// Optional. Background color of the pass, specified as an CSS-style RGB triple. For example, rgb(23, 187, 82).
    /// </summary>
    public string BackgroundColor { get; set; } = null!;

    /// <summary>
    /// Optional. Color of the label text, specified as a CSS-style RGB triple. For example, rgb(255, 255, 255).
    /// If omitted, the label color is determined automatically.
    /// </summary>
    public string LabelColor { get; set; } = null!;

    /// <summary>
    /// Optional. Text displayed next to the logo on the pass.
    /// </summary>
    public string LogoText { get; set; } = null!;

    /// <summary>
    /// Optional. If true, the strip image is displayed without a shine effect. The default value is false.
    /// </summary>
    public bool? SuppressStripShine { get; set; } = null!;

    /// <summary>
    /// Optional. The semantic tags to add to the pass. Read more about them here https://developer.apple.com/documentation/walletpasses/semantictags
    /// </summary>
    public SemanticTags SemanticTags { get; } = new();

    /// <summary>
    /// Optional. Fields to be displayed prominently on the front of the pass.
    /// </summary>
    public List<Field> HeaderFields { get; private set; } = new();

    /// <summary>
    /// Optional. Fields to be displayed prominently on the front of the pass.
    /// </summary>
    public List<Field> PrimaryFields { get; private set; } = new();

    /// <summary>
    /// Optional. Fields to be displayed on the front of the pass.
    /// </summary>
    public List<Field> SecondaryFields { get; private set; } = new();

    /// <summary>
    /// Optional. Additional fields to be displayed on the front of the pass.
    /// </summary>
    public List<Field> AuxiliaryFields { get; private set; } = new();

    /// <summary>
    /// Optional. Information about fields that are displayed on the back of the pass.
    /// </summary>
    public List<Field> BackFields { get; private set; } = new();

    /// <summary>
    /// Optional. Information specific to barcodes.
    /// </summary>
    public Barcode Barcode { get; private set; } = null!;

    /// <summary>
    /// Required. Pass type.
    /// </summary>
    public PassStyle Style { get; set; } = PassStyle.Generic;

    /// <summary>
    /// Required for boarding passes; otherwise not allowed. Type of transit.
    /// </summary>
    public TransitType? TransitType { get; set; } = null!;

    /// <summary>
    /// Optional for event tickets and boarding passes; otherwise not allowed. Identifier used to group related passes
    /// </summary>
    public string GroupingIdentifier { get; set; } = null!;

    #endregion

    #region Relevance Keys

    /// <summary>
    /// Optional. Date and time when the pass becomes relevant. For example, the start time of a movie.
    /// </summary>
    public DateTimeOffset? RelevantDate { get; set; } = null!;

    /// <summary>
    /// Optional. Locations where the passisrelevant. For example, the location of your store.
    /// </summary>
    public List<RelevantLocation> RelevantLocations { get; private set; } = new();

    /// <summary>
    /// Optional. Beacons marking locations where the pass is relevant.
    /// </summary>
    public List<RelevantBeacon> RelevantBeacons { get; private set; } = new();

    /// <summary>
    /// Optional. Maximum distance in meters from a relevant latitude and longitude that the pass is relevant
    /// </summary>
    public int? MaxDistance { get; set; } = null!;

    #endregion

    #region Certificates

    /// <summary>
    /// A byte array containing the PassKit certificate
    /// </summary>
    public X509Certificate2 PassbookCertificate { get; set; } = null!;

    /// <summary>
    /// A byte array containing the Apple WWDRCA X509 certificate
    /// </summary>
    public X509Certificate2 AppleWWDRCACertificate { get; set; } = null!;

    #endregion

    #region Web Service Keys

    /// <summary>
    /// The authentication token to use with the web service.
    /// </summary>
    public string AuthenticationToken { get; set; } = null!;

    /// <summary>
    /// The URL of a web service that conforms to the API described in Pass Kit Web Service Reference.
    /// The web service must use the HTTPS protocol and includes the leading https://.
    /// On devices configured for development, there is UI in Settings to allow HTTP web services.
    /// </summary>
    public string WebServiceUrl { get; set; } = null!;

    #endregion

    #region Associated App Keys

    public List<long> AssociatedStoreIdentifiers { get; set; } = new();

    public string AppLaunchUrl { get; set; } = null!;

    #endregion

    #region Barcodes

    public List<Barcode> Barcodes { get; private set; } = new();

    #endregion

    #region User Info Keys

    public Dictionary<string, object> UserInfo { get; set; } = new();

    #endregion

    #region Localization

    public Dictionary<string, Dictionary<string, string>> Localizations { get; set; } = new();

    #endregion

    #region NFC

    public Nfc Nfc { get; set; } = null!;

    #endregion

    #region Helpers and Serialization

    public PassGeneratorRequest AddHeaderField(Field field)
    {
        EnsureFieldKeyIsUnique(field.Key);
        HeaderFields.Add(field);

        return this;
    }

    public PassGeneratorRequest AddPrimaryField(Field field)
    {
        EnsureFieldKeyIsUnique(field.Key);
        PrimaryFields.Add(field);

        return this;
    }

    public PassGeneratorRequest AddSecondaryField(Field field)
    {
        EnsureFieldKeyIsUnique(field.Key);
        SecondaryFields.Add(field);

        return this;
    }

    public PassGeneratorRequest AddAuxiliaryField(Field field)
    {
        EnsureFieldKeyIsUnique(field.Key);
        AuxiliaryFields.Add(field);

        return this;
    }

    public PassGeneratorRequest AddBackField(Field field)
    {
        EnsureFieldKeyIsUnique(field.Key);
        BackFields.Add(field);

        return this;
    }

    private PassGeneratorRequest EnsureFieldKeyIsUnique(string key)
    {
        if (HeaderFields.Any(x => x.Key == key) ||
            PrimaryFields.Any(x => x.Key == key) ||
            SecondaryFields.Any(x => x.Key == key) ||
            AuxiliaryFields.Any(x => x.Key == key) ||
            BackFields.Any(x => x.Key == key))
        {
            throw new DuplicateFieldKeyException(key);
        }

        return this;
    }

    public PassGeneratorRequest AddBarcode(BarcodeType type, string message, string encoding, string alternateText)
    {
        Barcodes.Add(new Barcode(type, message, encoding, alternateText));

        return this;
    }

    public PassGeneratorRequest AddBarcode(BarcodeType type, string message, string encoding)
    {
        Barcodes.Add(new Barcode(type, message, encoding));

        return this;
    }

    public PassGeneratorRequest SetBarcode(BarcodeType type, string message, string encoding, string alternateText = null)
    {
        Barcode = new Barcode(type, message, encoding, alternateText);

        return this;
    }

    public PassGeneratorRequest AddLocation(double latitude, double longitude)
    {
        AddLocation(latitude, longitude, null);

        return this;
    }

    public PassGeneratorRequest AddLocation(double latitude, double longitude, string relevantText)
    {
        RelevantLocations.Add(new RelevantLocation {Latitude = latitude, Longitude = longitude, RelevantText = relevantText});

        return this;
    }

    public PassGeneratorRequest AddBeacon(string proximityUuid, string relevantText)
    {
        RelevantBeacons.Add(new RelevantBeacon {ProximityUuid = proximityUuid, RelevantText = relevantText});

        return this;
    }

    public PassGeneratorRequest AddBeacon(string proximityUuid, string relevantText, int major)
    {
        RelevantBeacons.Add(new RelevantBeacon {ProximityUuid = proximityUuid, RelevantText = relevantText, Major = major});

        return this;
    }

    public PassGeneratorRequest AddBeacon(string proximityUuid, string relevantText, int major, int minor)
    {
        RelevantBeacons.Add(new RelevantBeacon {ProximityUuid = proximityUuid, RelevantText = relevantText, Major = major, Minor = minor});

        return this;
    }

    public PassGeneratorRequest AddLocalization(string languageCode, string key, string value)
    {
        if (!Localizations.TryGetValue(languageCode, out var values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Localizations.Add(languageCode, values);
        }

        values[key] = value;

        return this;
    }

    public virtual void PopulateFields()
    {
        // NO OP.
    }

    public void Write(JsonWriter writer)
    {
        PopulateFields();

        writer.WriteStartObject();

        Trace.TraceInformation("Writing semantics..");
        WriteSemantics(writer);
        Trace.TraceInformation("Writing standard keys..");
        WriteStandardKeys(writer);
        Trace.TraceInformation("Writing user information..");
        WriteUserInfo(writer);
        Trace.TraceInformation("Writing relevance keys..");
        WriteRelevanceKeys(writer);
        Trace.TraceInformation("Writing appearance keys..");
        WriteAppearanceKeys(writer);
        Trace.TraceInformation("Writing expiration keys..");
        WriteExpirationKeys(writer);
        Trace.TraceInformation("Writing barcode keys..");
        WriteBarcodes(writer);

        if (Nfc != null)
        {
            Trace.TraceInformation("Writing NFC fields");
            WriteNfcKeys(writer);
        }

        Trace.TraceInformation("Opening style section..");
        OpenStyleSpecificKey(writer);

        Trace.TraceInformation("Writing header fields");
        WriteSection(writer, "headerFields", HeaderFields);
        Trace.TraceInformation("Writing primary fields");
        WriteSection(writer, "primaryFields", PrimaryFields);
        Trace.TraceInformation("Writing secondary fields");
        WriteSection(writer, "secondaryFields", SecondaryFields);
        Trace.TraceInformation("Writing auxiliary fields");
        WriteSection(writer, "auxiliaryFields", AuxiliaryFields);
        Trace.TraceInformation("Writing back fields");
        WriteSection(writer, "backFields", BackFields);

        if (Style == PassStyle.BoardingPass)
        {
            writer.WritePropertyName("transitType");
            writer.WriteValue(TransitType.ToString());
        }

        Trace.TraceInformation("Closing style section..");
        CloseStyleSpecificKey(writer);

        WriteBarcode(writer);
        WriteUrls(writer);

        writer.WriteEndObject();
    }

    private void WriteRelevanceKeys(JsonWriter writer)
    {
        if (RelevantDate.HasValue)
        {
            writer.WritePropertyName("relevantDate");
            writer.WriteValue(RelevantDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
        }

        if (MaxDistance.HasValue)
        {
            writer.WritePropertyName("maxDistance");
            writer.WriteValue(MaxDistance.Value.ToString());
        }

        if (RelevantLocations.Count > 0)
        {
            writer.WritePropertyName("locations");
            writer.WriteStartArray();

            foreach (var location in RelevantLocations)
            {
                location.Write(writer);
            }

            writer.WriteEndArray();
        }

        if (RelevantBeacons.Count > 0)
        {
            writer.WritePropertyName("beacons");
            writer.WriteStartArray();

            foreach (var beacon in RelevantBeacons)
            {
                beacon.Write(writer);
            }

            writer.WriteEndArray();
        }
    }

    private void WriteUrls(JsonWriter writer)
    {
        if (string.IsNullOrEmpty(AuthenticationToken)) return;
        writer.WritePropertyName("authenticationToken");
        writer.WriteValue(AuthenticationToken);
        writer.WritePropertyName("webServiceURL");
        writer.WriteValue(WebServiceUrl);
    }

    private void WriteBarcode(JsonWriter writer)
    {
        if (Barcode == null) return;
        writer.WritePropertyName("barcode");
        Barcode.Write(writer);
    }

    private void WriteBarcodes(JsonWriter writer)
    {
        if (Barcodes.Count <= 0) return;
        writer.WritePropertyName("barcodes");
        writer.WriteStartArray();

        foreach (var barcode in Barcodes)
            barcode.Write(writer);

        writer.WriteEndArray();
    }

    private void WriteSemantics(JsonWriter writer)
    {
        SemanticTags.Write(writer);
    }

    private void WriteStandardKeys(JsonWriter writer)
    {
        writer.WritePropertyName("passTypeIdentifier");
        writer.WriteValue(PassTypeIdentifier);

        writer.WritePropertyName("formatVersion");
        writer.WriteValue(FormatVersion);

        writer.WritePropertyName("serialNumber");
        writer.WriteValue(SerialNumber);

        writer.WritePropertyName("description");
        writer.WriteValue(Description);

        writer.WritePropertyName("organizationName");
        writer.WriteValue(OrganizationName);

        writer.WritePropertyName("teamIdentifier");
        writer.WriteValue(TeamIdentifier);

        writer.WritePropertyName("sharingProhibited");
        writer.WriteValue(SharingProhibited);

        if (!string.IsNullOrEmpty(LogoText))
        {
            writer.WritePropertyName("logoText");
            writer.WriteValue(LogoText);
        }

        if (AssociatedStoreIdentifiers.Count > 0)
        {
            writer.WritePropertyName("associatedStoreIdentifiers");

            writer.WriteStartArray();

            foreach (var storeIdentifier in AssociatedStoreIdentifiers)
            {
                writer.WriteValue(storeIdentifier);
            }

            writer.WriteEndArray();
        }

        if (string.IsNullOrEmpty(AppLaunchUrl)) return;
        writer.WritePropertyName("appLaunchURL");
        writer.WriteValue(AppLaunchUrl);
    }

    private void WriteUserInfo(JsonWriter writer)
    {
        if (UserInfo == null) return;
        writer.WritePropertyName("userInfo");
        writer.WriteRawValue(JsonConvert.SerializeObject(UserInfo));
    }

    private void WriteAppearanceKeys(JsonWriter writer)
    {
        if (!string.IsNullOrEmpty(ForegroundColor))
        {
            writer.WritePropertyName("foregroundColor");
            writer.WriteValue(ConvertColor(ForegroundColor));
        }

        if (!string.IsNullOrEmpty(BackgroundColor))
        {
            writer.WritePropertyName("backgroundColor");
            writer.WriteValue(ConvertColor(BackgroundColor));
        }

        if (!string.IsNullOrEmpty(LabelColor))
        {
            writer.WritePropertyName("labelColor");
            writer.WriteValue(ConvertColor(LabelColor));
        }

        if (SuppressStripShine.HasValue)
        {
            writer.WritePropertyName("suppressStripShine");
            writer.WriteValue(SuppressStripShine.Value);
        }

        if (string.IsNullOrEmpty(GroupingIdentifier)) return;
        writer.WritePropertyName("groupingIdentifier");
        writer.WriteValue(GroupingIdentifier);
    }

    private void WriteExpirationKeys(JsonWriter writer)
    {
        if (ExpirationDate.HasValue)
        {
            writer.WritePropertyName("expirationDate");
            writer.WriteValue(ExpirationDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
        }

        if (!Voided.HasValue) return;
        writer.WritePropertyName("voided");
        writer.WriteValue(Voided.Value);
    }

    private void OpenStyleSpecificKey(JsonWriter writer)
    {
        var key = Style.ToString();

        writer.WritePropertyName(char.ToLowerInvariant(key[0]) + key[1..]);
        writer.WriteStartObject();
    }

    private static void CloseStyleSpecificKey(JsonWriter writer)
    {
        writer.WriteEndObject();
    }

    private static void WriteSection(JsonWriter writer, string sectionName, List<Field> fields)
    {
        writer.WritePropertyName(sectionName);
        writer.WriteStartArray();

        foreach (var field in fields)
        {
            field.Write(writer);
        }

        writer.WriteEndArray();
    }

    private void WriteNfcKeys(JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Nfc.Message)) return;
        writer.WritePropertyName("nfc");
        writer.WriteStartObject();
        writer.WritePropertyName("message");
        writer.WriteValue(Nfc.Message);

        if (!string.IsNullOrEmpty(Nfc.EncryptionPublicKey))
        {
            writer.WritePropertyName("encryptionPublicKey");
            writer.WriteValue(Nfc.EncryptionPublicKey);
        }

        writer.WriteEndObject();
    }

    private static string ConvertColor(string color)
    {
        if (string.IsNullOrEmpty(color) || color[..1] != "#") return color;
        int r, g, b;

        switch (color.Length)
        {
            case 3:
                r = int.Parse(color.Substring(1, 1), NumberStyles.HexNumber);
                g = int.Parse(color.Substring(2, 1), NumberStyles.HexNumber);
                b = int.Parse(color.Substring(3, 1), NumberStyles.HexNumber);
                break;
            case >= 6:
                r = int.Parse(color.Substring(1, 2), NumberStyles.HexNumber);
                g = int.Parse(color.Substring(3, 2), NumberStyles.HexNumber);
                b = int.Parse(color.Substring(5, 2), NumberStyles.HexNumber);
                break;
            default:
                throw new ArgumentException("use #rgb or #rrggbb for color values", color);
        }

        return $"rgb({r},{g},{b})";
    }

    #endregion
}
