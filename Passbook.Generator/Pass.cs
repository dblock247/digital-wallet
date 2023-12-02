using System.IO;

namespace Passbook.Generator;

public class Pass
{
    private readonly string _packagePathAndName;

    public Pass(string packagePathAndName)
    {
        _packagePathAndName = packagePathAndName;
    }

    public byte[] GetPackage()
    {
        var contents = File.ReadAllBytes(_packagePathAndName);
        return contents;
    }

    public string PackageDirectory => Path.GetDirectoryName(_packagePathAndName);
}
