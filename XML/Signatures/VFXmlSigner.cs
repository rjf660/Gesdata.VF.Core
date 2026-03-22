using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Crypto;
using FirmaXadesNetCore.Signature.Parameters;
using Gesdata.Comun.Xml.Signatures;
using Gesdata.Comun.Xml.Signatures.Policies;
using Gesdata.VF.Core.Configuration;
// Alias para resolver conflicto de nombres
using XadesNetCorePolicyInfo = FirmaXadesNetCore.Signature.Parameters.SignaturePolicyInfo;

namespace Gesdata.VF.Core.XML.Signatures
{
    /// <summary>
    /// UtilComun para firma XML (enveloped) y verificación/visualización de firmas.
    /// ✅ REFACTORIZADO: Ahora usa infraestructura común de Gesdata.Comun.Xml.Signatures.
    /// - Verificación: delega a XmlSignatureVerifier
    /// - Análisis: delega a XmlSignatureAnalyzer
    /// - Política: usa VeriFactuPolicyProvider
    /// </summary>
    public static class VFXmlSigner
    {
        static VFXmlSigner()
        {
            // Asegurar mapeo seguro para RSA-SHA256 y evitar overrides incompatibles.
            Gesdata.VF.Core.Security.CryptoConfigInitializer.RegisterXmlDsigAlgorithms();
        }

        [Flags]
        public enum RegistroNodeKinds
        {
            None = 0,
            RegistroAlta = 1 << 0,
            RegistroAnulacion = 1 << 1,
            RegistroEvento = 1 << 2,
            All = RegistroAlta | RegistroAnulacion | RegistroEvento,
        }

        // Firma sencilla Enveloped
        public static string SignXml(string xml, X509Certificate2 cert)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return xml;
            ArgumentNullException.ThrowIfNull(cert);
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xml);
            var rsa = cert.GetRSAPrivateKey() ??
                throw new InvalidOperationException("El certificado no contiene clave privada RSA para firmar.");
            SignedXml signedXml = new(doc) { SigningKey = rsa };
            Reference reference = new() { Uri = string.Empty };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);
            KeyInfo keyInfo = new();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            return doc.OuterXml;
        }

        /// <summary>
        /// Verificación booleana de firma XML.
        /// ✅ Delega a XmlSignatureVerifier común de Gesdata.Comun.
        /// </summary>
        public static bool VerifySignature(string signedXml)
        {
            return XmlSignatureVerifier.Instance.Verify(signedXml);
        }

        /// <summary>
        /// Analiza una firma XML y devuelve información detallada.
        /// ✅ Delega a XmlSignatureAnalyzer común de Gesdata.Comun.
        /// </summary>
        /// <returns>Detalles de la firma desde infraestructura común</returns>
        public static Comun.Xml.Signatures.SignatureDetails AnalyzeSignature(string xml)
        {
            return XmlSignatureAnalyzer.Analyze(xml);
        }

        /// <summary>
        /// Genera un reporte legible de la firma.
        /// ✅ Delega a XmlSignatureAnalyzer común de Gesdata.Comun.
        /// </summary>
        public static string FormatSignatureReport(Comun.Xml.Signatures.SignatureDetails details)
        {
            return XmlSignatureAnalyzer.FormatReport(details);
        }

        public static Result TryFirmarXml(string xmlPath, string certPath, string password, string outputPath)
        {
            return ErrorHandling.Try(
                () => FirmarXml(xmlPath, certPath, password, outputPath),
                nameof(VFXmlSigner) + ".FirmarXml");
        }

        // Sign XAdES EPES (compatibilidad)
        public static string SignAllRegistrosXadesEpes(string xml, X509Certificate2 cert, bool useEpesPolicy = true)
        {
            return SignAllRegistrosXadesEpes(
                xml,
                cert,
                useEpesPolicy,
                null,
                RegistroNodeKinds.All,
                relocateSignatureInEvento: true,
                validateAfterSign: true);
        }

        // Sign XAdES EPES con política configurable (compatibilidad)
        public static string SignAllRegistrosXadesEpes(
            string xml,
            X509Certificate2 cert,
            bool useEpesPolicy,
            SignaturePolicySettings policySettings)
        {
            return SignAllRegistrosXadesEpes(
                xml,
                cert,
                useEpesPolicy,
                policySettings,
                RegistroNodeKinds.All,
                relocateSignatureInEvento: true,
                validateAfterSign: true);
        }

        /// <summary>
        /// Firma todos o algunos nodos de registros (RegistroAlta/RegistroAnulacion/RegistroEvento) con XAdES EPES.
        /// Permite seleccionar qué nodos firmar y si reubicar ds:Signature bajo 'Evento'. Opcionalmente valida tras
        /// firmar.
        /// </summary>
        public static string SignAllRegistrosXadesEpes(
            string xml,
            X509Certificate2 cert,
            bool useEpesPolicy,
            SignaturePolicySettings policySettings,
            RegistroNodeKinds nodesToSign,
            bool relocateSignatureInEvento,
            bool validateAfterSign)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return xml;
            ArgumentNullException.ThrowIfNull(cert);
            EnsureRsaKeyLength(cert, minBits: 2048);
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xml);
            var allTargets = doc.SelectNodes(
                "//*[local-name()='RegistroAlta' or local-name()='RegistroAnulacion' or local-name()='RegistroEvento']");
            if (allTargets is null || allTargets.Count == 0)
                return xml;

            foreach (XmlElement target in allTargets)
            {
                bool isAlta = target.LocalName == "RegistroAlta";
                bool isAnul = target.LocalName == "RegistroAnulacion";
                bool isEvt = target.LocalName == "RegistroEvento";
                if ((isAlta &&
                    !nodesToSign.HasFlag(RegistroNodeKinds.RegistroAlta)) ||
                    (isAnul &&
                    !nodesToSign.HasFlag(RegistroNodeKinds.RegistroAnulacion)) ||
                    (isEvt &&
                    !nodesToSign.HasFlag(RegistroNodeKinds.RegistroEvento)))
                {
                    continue;
                }
                var signedFragment = SignFragmentEpes(
                    target.OuterXml,
                    cert,
                    isEvento: isEvt,
                    useEpesPolicy: useEpesPolicy,
                    policySettings: policySettings,
                    relocateInEvento: relocateSignatureInEvento);
                if (validateAfterSign)
                {
                    // Validar firma del fragmento usando verificador común
                    if (!VerifySignature(signedFragment))
                    {
                        throw new InvalidOperationException($"Firma XAdES no válida para nodo {target.LocalName}.");
                    }
                }
                var fragDoc = new XmlDocument { PreserveWhitespace = true };
                fragDoc.LoadXml(signedFragment);
                var newElem = doc.ImportNode(fragDoc.DocumentElement!, deep: true);
                target.ParentNode!.ReplaceChild(newElem, target);
            }
            return doc.OuterXml;
        }

        private static string SignFragmentEpes(
            string fragmentXml,
            X509Certificate2 cert,
            bool isEvento,
            bool useEpesPolicy = true,
            SignaturePolicySettings policySettings = null,
            bool relocateInEvento = true)
        {
            var service = new XadesService();
            var parameters = new SignatureParameters
            {
                SigningDate = DateTime.UtcNow,
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                DataFormat = new DataFormat { MimeType = "text/xml" },
                Signer = new Signer(cert),
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                DigestMethod = DigestMethod.SHA256,
            };

            if (useEpesPolicy)
            {
                // ✅ Usar VeriFactuPolicyProvider común de Gesdata.Comun
                var policy = VeriFactuPolicyProvider.Instance;

                var alg = policySettings?.HashAlgorithm?.ToUpperInvariant() switch
                {
                    "SHA256" => DigestMethod.SHA256,
                    _ => DigestMethod.SHA1,
                };

                var oid = string.IsNullOrWhiteSpace(policySettings?.Oid) ? policy.Identifier : policySettings!.Oid;
                var url = string.IsNullOrWhiteSpace(policySettings?.Url) ? policy.Url : policySettings!.Url;
                var hash = string.IsNullOrWhiteSpace(policySettings?.PolicyHashBase64)
                    ? policy.Hash
                    : policySettings!.PolicyHashBase64;

                // Usar alias para el tipo de FirmaXadesNetCore
                parameters.SignaturePolicyInfo = new XadesNetCorePolicyInfo
                {
                    PolicyIdentifier = oid,
                    PolicyHash = hash,
                    PolicyUri = url,
                    PolicyDigestAlgorithm = alg,
                };
            }

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(fragmentXml));
            using var outMs = new MemoryStream();
            var signed = service.Sign(ms, parameters);
            signed.Save(outMs);
            var signedXml = Encoding.UTF8.GetString(outMs.ToArray());
            if (!isEvento || !relocateInEvento)
                return signedXml;
            // Use helper to ensure correct placement before OtrosDatosEvento
            return VFSignatureRewriter.MoveSignatureBeforeOtrosDatosEvento(signedXml);
        }

        /// <summary>
        /// Firma un archivo XML en disco con un certificado (PFX) y guarda la salida firmada.
        /// </summary>
        public static void FirmarXml(string xmlPath, string certPath, string password, string outputPath)
        {
            var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, password, X509KeyStorageFlags.Exportable);
            XmlDocument doc = new() { PreserveWhitespace = true, };
            doc.Load(xmlPath);

            SignedXml signedXml = new(doc) { SigningKey = cert.GetRSAPrivateKey(), };

            Reference reference = new() { Uri = string.Empty, };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            KeyInfo keyInfo = new();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            doc.Save(outputPath);
        }

        private static void EnsureRsaKeyLength(X509Certificate2 cert, int minBits)
        {
            using var rsa = cert.GetRSAPrivateKey() ??
                throw new InvalidOperationException("El certificado no contiene clave privada RSA para firmar.");
            var size = rsa.KeySize;
            if (size < minBits)
            {
                throw new InvalidOperationException(
                    $"La clave RSA del certificado es de {size} bits y debe ser >= {minBits} bits.");
            }
        }

        public static string SignXmlXades(string xml, X509Certificate2 cert)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return xml;
            ArgumentNullException.ThrowIfNull(cert);
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xml);
            var service = new XadesService();
            var parameters = new SignatureParameters
            {
                SigningDate = DateTime.UtcNow,
                SignaturePolicyInfo = null,
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                DataFormat = new DataFormat { MimeType = "text/xml" },
                Signer = new Signer(cert),
            };
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            using var outMs = new MemoryStream();
            var signed = service.Sign(ms, parameters);
            signed.Save(outMs);
            return Encoding.UTF8.GetString(outMs.ToArray());
        }
    }
}
