using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Passbook.Generator.Exceptions;
using Passbook.Generator.Extensions;

namespace Passbook.Generator;

public class PassGenerator
{
    private byte[] _passFile;
    private byte[] _signatureFile;
    private byte[] _manifestFile;
    private Dictionary<string, byte[]> _localizationFiles;
    private byte[] _pkPassFile;
    private byte[] _pkPassBundle;
    private const string PassTypePrefix = "Pass Type ID: ";

    /// <summary>
    /// Creates a byte array which contains one pkpass file
    /// </summary>
    /// <param name="generatorRequest">
    /// An instance of a PassGeneratorRequest</param>
    /// <returns>
    /// A byte array which contains a zipped pkpass file.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public byte[] Generate(PassGeneratorRequest generatorRequest)
    {
        if (generatorRequest == null)
            throw new ArgumentNullException(nameof(generatorRequest)
                , "You must pass an instance of PassGeneratorRequest");

        CreatePackage(generatorRequest);
        ZipPackage(generatorRequest);

        return _pkPassFile;
    }

    /// <summary>
    /// Creates a byte array that can contains a .pkpasses file for bundling multiple passes together
    /// </summary>
    /// <param name="generatorRequests">
    /// A list of PassGeneratorRequest objects
    /// </param>
    /// <returns>
    /// A byte array which contains a zipped pkpasses file.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="System.ArgumentException"></exception>
    public byte[] Generate(IReadOnlyList<PassGeneratorRequest> generatorRequests)
    {
        if (generatorRequests == null)
            throw new ArgumentNullException(nameof(generatorRequests)
                , "You must pass an instance of IReadOnlyList containing at least one PassGeneratorRequest");

        if (!generatorRequests.Any())
            throw new ArgumentException(
                "The IReadOnlyList must contain at least one PassGeneratorRequest"
                , nameof(generatorRequests));

        ZipBundle(generatorRequests);

        return _pkPassBundle;
    }

    private void ZipBundle(IEnumerable<PassGeneratorRequest> generatorRequests)
    {
        using var zipToOpen = new MemoryStream();
        using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create, true);
        var i = 0;

        foreach (var generatorRequest in generatorRequests)
        {
            var zipEntry = archive.CreateEntry($"pass{i++}.pkpass");

            CreatePackage(generatorRequest);
            ZipPackage(generatorRequest);

            using var originalFileStream = new MemoryStream(_pkPassFile);
            using var zipEntryStream = zipEntry.Open();
            originalFileStream.CopyTo(zipEntryStream);
        }

        _pkPassBundle = zipToOpen.ToArray();

        zipToOpen.Flush();
    }

    private void ZipPackage(PassGeneratorRequest request)
    {
        using var zipToOpen = new MemoryStream();
        using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update, true);

        foreach (var image in request.Images)
        {
            var imageEntry = archive.CreateEntry(image.Key.ToFilename());
            using var writer = new BinaryWriter(imageEntry.Open());
            writer.Write(image.Value);
            writer.Flush();
        }

        foreach (var (key, value) in _localizationFiles)
        {
            var localizationEntry = archive.CreateEntry($"{key.ToLower()}.lproj/pass.strings");
            using var writer = new BinaryWriter(localizationEntry.Open());
            writer.Write(value);
            writer.Flush();
        }

        var passJsonEntry = archive.CreateEntry(@"pass.json");
        using var passWriter = new BinaryWriter(passJsonEntry.Open());
        passWriter.Write(_passFile);
        passWriter.Flush();

        var manifestJsonEntry = archive.CreateEntry(@"manifest.json");
        using var manifestWriter = new BinaryWriter(manifestJsonEntry.Open());
        manifestWriter.Write(_manifestFile);
        manifestWriter.Flush();

        var signatureEntry = archive.CreateEntry(@"signature");
        using var signatureWriter = new BinaryWriter(signatureEntry.Open());
        signatureWriter.Write(_signatureFile);
        signatureWriter.Flush();

        _pkPassFile = zipToOpen.ToArray();
        zipToOpen.Flush();
    }

    private void CreatePackage(PassGeneratorRequest request)
    {
        ValidateCertificates(request);
        CreatePassFile(request);
        GenerateLocalizationFiles(request);
        GenerateManifestFile(request);
    }

    private static void ValidateCertificates(PassGeneratorRequest request)
    {
        if (request.PassbookCertificate == null)
            throw new FileNotFoundException(
                "Certificate could not be found. Please ensure the thumbprint and cert location values are correct.");

        if (request.AppleWWDRCACertificate == null)
            throw new FileNotFoundException(
                "Apple Certificate could not be found. Please download it from https://www.apple.com/certificateauthority and install it into your PERSONAL or LOCAL MACHINE certificate store.");

        var passTypeIdentifier = request.PassbookCertificate.GetNameInfo(X509NameType.SimpleName, false);

        if (passTypeIdentifier.StartsWith(PassTypePrefix, StringComparison.OrdinalIgnoreCase))
        {
            passTypeIdentifier = passTypeIdentifier.Substring(PassTypePrefix.Length);

            if (!string.IsNullOrEmpty(passTypeIdentifier) && !string.Equals(request.PassTypeIdentifier,
                    passTypeIdentifier, StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(request.PassTypeIdentifier))
                {
                    Trace.TraceWarning(
                        "Configured passTypeIdentifier {0} does not match pass certificate {1}, correcting.",
                        request.PassTypeIdentifier, passTypeIdentifier);
                }

                request.PassTypeIdentifier = passTypeIdentifier;
            }
        }

        var nameParts = Regex.Matches(request.PassbookCertificate.SubjectName.Name,
                @"(?<key>[^=,\s]+)\=*(?<value>("".+""|[^,])+)")
            .ToDictionary(
                m => m.Groups["key"].Value,
                m => m.Groups["value"].Value);

        if (nameParts.TryGetValue("OU", out var teamIdentifier))
        {
            if (!string.IsNullOrEmpty(teamIdentifier) &&
                !string.Equals(request.TeamIdentifier, teamIdentifier, StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(request.TeamIdentifier))
                {
                    Trace.TraceWarning("Configured teamIdentifier {0} does not match pass certificate {1}, correcting.",
                        request.TeamIdentifier, teamIdentifier);
                }

                request.TeamIdentifier = teamIdentifier;
            }
        }
    }

    private void CreatePassFile(PassGeneratorRequest request)
    {
        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);
        using var writer = new JsonTextWriter(sr);

        writer.Formatting = Formatting.Indented;

        Trace.TraceInformation("Writing JSON...");
        request.Write(writer);

        _passFile = ms.ToArray();
    }

    private void GenerateLocalizationFiles(PassGeneratorRequest request)
    {
        _localizationFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var localization in request.Localizations)
        {
            using var ms = new MemoryStream();
            using var sr = new StreamWriter(ms, Encoding.UTF8);
            foreach (var (key, value) in localization.Value)
            {
                sr.WriteLine("\"{0}\" = \"{1}\";\n", key, value);
            }

            sr.Flush();
            _localizationFiles.Add(localization.Key, ms.ToArray());
        }
    }

    private void GenerateManifestFile(PassGeneratorRequest request)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        using var jsonWriter = new JsonTextWriter(sw);

        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.WriteStartObject();

        var hash = GetHashForBytes(_passFile);
        jsonWriter.WritePropertyName(@"pass.json");
        jsonWriter.WriteValue(hash);

        foreach (var image in request.Images)
        {
            try
            {
                hash = GetHashForBytes(image.Value);
                jsonWriter.WritePropertyName(image.Key.ToFilename());
                jsonWriter.WriteValue(hash);
            }
            catch (Exception exception)
            {
                throw new Exception($"Unexpect error writing image on manifest file. Image Key: \"{image.Key}\"",
                    exception);
            }
        }

        foreach (var localization in _localizationFiles)
        {
            hash = GetHashForBytes(localization.Value);
            jsonWriter.WritePropertyName($"{localization.Key.ToLower()}.lproj/pass.strings");
            jsonWriter.WriteValue(hash);
        }

        _manifestFile = ms.ToArray();
        SignManifestFile(request);
    }

    private void SignManifestFile(PassGeneratorRequest request)
    {
        Trace.TraceInformation("Signing the manifest file...");

        try
        {
            var contentInfo = new ContentInfo(_manifestFile);
            var signing = new SignedCms(contentInfo, true);
            var signer = new CmsSigner(SubjectIdentifierType.SubjectKeyIdentifier, request.PassbookCertificate)
            {
                IncludeOption = X509IncludeOption.None
            };

            Trace.TraceInformation("Fetching Apple Certificate for signing..");
            Trace.TraceInformation("Constructing the certificate chain..");
            signer.Certificates.Add(request.AppleWWDRCACertificate);
            signer.Certificates.Add(request.PassbookCertificate);

            signer.SignedAttributes.Add(new Pkcs9SigningTime());

            Trace.TraceInformation("Processing the signature..");
            signing.ComputeSignature(signer);

            _signatureFile = signing.Encode();

            Trace.TraceInformation("The file has been successfully signed!");
        }
        catch (Exception exp)
        {
            Trace.TraceError("Failed to sign the manifest file: [{0}]", exp.Message);
            throw new ManifestSigningException("Failed to sign manifest", exp);
        }
    }

    private static string GetHashForBytes(byte[] bytes)
    {
        return Convert.ToHexString(SHA1.HashData(bytes));
    }
}
