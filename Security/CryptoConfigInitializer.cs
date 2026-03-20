using Gesdata.Comun.Logging;
using System.Security.Cryptography; // añadido para CryptoConfig y SignatureDescription
using System.Security.Cryptography.Xml;

namespace Gesdata.VF.Core.Security;

public static class CryptoConfigInitializer
{
    private static readonly LoggingService _loggingService = new();
    /// <summary>
    /// Ensure a compatible SignatureDescription is registered for RSA-SHA256, overriding any incompatible ones. Call
    /// early at app startup, before signing or verifying.
    /// </summary>
    public static void RegisterXmlDsigAlgorithms()
    {
        try
        {
            var uri = SignedXml.XmlDsigRSASHA256Url; // "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
            CryptoConfig.AddAlgorithm(typeof(RsaPkcs1Sha256SignatureDescription), uri);
            // Optional: verify mapping
            if (CryptoConfig.CreateFromName(uri) is not SignatureDescription sd)
            {
                _loggingService.LogWarning("Gesdata.VF.Core.Security", "[CryptoConfig] Warning: RSA-SHA256 SignatureDescription could not be created after registration.");
            }
        }
        catch (PlatformNotSupportedException)
        {
            _loggingService.LogError("Gesdata.VF.Core.Security", "[CryptoConfig] AddAlgorithm not supported on this platform; relying on BCL mapping.");
        }
    }
}
