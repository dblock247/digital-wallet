using System;
using System.Runtime.Serialization;

namespace Passbook.Generator.Exceptions;

public class ManifestSigningException : Exception
{
    public ManifestSigningException()
    {
    }

    public ManifestSigningException(string message) : base(message)
    {
    }

    public ManifestSigningException(string message, Exception inner) : base(message, inner)
    {
    }
}
