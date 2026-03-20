using System.Security.Cryptography;

namespace Gesdata.VF.Core.Security;

/// <summary>
/// Safe RSA-PKCS1 SHA-256 SignatureDescription that works across modern .NET runtimes. Uses base RSA type and assembly-
/// qualified names to avoid Type.GetType(...) returning null.
/// </summary>
public sealed class RsaPkcs1Sha256SignatureDescription : SignatureDescription
{
    public RsaPkcs1Sha256SignatureDescription()
    {
        KeyAlgorithm = typeof(RSA).AssemblyQualifiedName;
        DigestAlgorithm = typeof(SHA256).AssemblyQualifiedName;
        FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).AssemblyQualifiedName;
        DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).AssemblyQualifiedName;
    }

    public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var rsa = (RSA)key;
        var formatter = new RSAPKCS1SignatureFormatter(rsa);
        formatter.SetHashAlgorithm(nameof(SHA256));
        return formatter;
    }

    public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var rsa = (RSA)key;
        var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
        deformatter.SetHashAlgorithm(nameof(SHA256));
        return deformatter;
    }

    public override HashAlgorithm CreateDigest() => SHA256.Create();
}
