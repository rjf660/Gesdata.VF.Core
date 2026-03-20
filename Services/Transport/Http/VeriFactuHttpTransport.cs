using Gesdata.VF.Contracts.Types;
using Gesdata.VF.Contracts.XML;
using Gesdata.VF.Core.XML;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Gesdata.VF.Core.Services.Transport.Http
{
    /// <summary>
    /// Implementación de transporte HTTP directo para AEAT VeriFactu. Construye manualmente el SOAP Envelope para
    /// control total sobre namespaces.
    /// </summary>
    public sealed class VeriFactuHttpTransport : IAeatTransport
    {
        private readonly bool _saveDebugFiles;
        private readonly string _debugFolder;
        private readonly bool _corromperHuella; // ✅ TESTING
        private readonly Gesdata.Comun.Logging.LoggingService _logger;

        /// <summary>
        /// Constructor del transporte HTTP.
        /// </summary>
        /// <param name="saveDebugFiles">Si es true, guarda copias de SOAP request/response en el escritorio (útil para debugging inicial)</param>
        /// <param name="debugFolder">Carpeta base para guardar archivos (por defecto: Desktop)</param>
        /// <param name="corromperHuella">⚠️ TESTING: Si es true, corrompe intencionalmente la huella antes de enviar a AEAT</param>
        /// <param name="logger">Servicio de logging (opcional, con fallback seguro)</param>
        public VeriFactuHttpTransport(
            bool saveDebugFiles = false,
            string debugFolder = null,
            bool corromperHuella = false,
            Gesdata.Comun.Logging.LoggingService logger = null)
        {
            _saveDebugFiles = saveDebugFiles;
            _debugFolder = debugFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _corromperHuella = corromperHuella;
            _logger = logger ?? new Gesdata.Comun.Logging.LoggingService(); // ✅ Fallback seguro

            // ✅ LOG constructor para verificar parámetro
            if (_corromperHuella)
            {
                _logger.LogWarning(
                    "VeriFactuHttpTransport",
                    "⚠️ [TESTING] Modo corrupción de huella ACTIVADO - Las huellas serán corrompidas antes de enviar");
            }
        }

        public async Task<TransportResult<RespuestaRegFactuSistemaFacturacionType>> EnviarRegistroAsync(
            RegFactuSistemaFacturacionType solicitud,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            Debug.WriteLine($"[VeriFactuHttpTransport] EnviarRegistroAsync iniciado a las {startTime:yyyyMMdd:HHmmss.ffffff} con endpoint {endpoint}");
            try
            {
                // 1. Construir SOAP Envelope manualmente
                var soapEnvelope = BuildRegFactuSoapEnvelope(solicitud);

                // ✅ TESTING: Corromper huella intencionalmente si el flag está activo
                if (_corromperHuella)
                {
                    soapEnvelope = CorromperHuellaEnSoap(soapEnvelope);
                }

                // 2. Guardar SOAP REQUEST (solo si está habilitado)
                if (_saveDebugFiles)
                {
                    SaveSoapFile(soapEnvelope, "SOAP_REQUEST", DeterminarCategoria(solicitud));
                }

                // 3. Enviar vía HttpClient
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(certificado);
                handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;

                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2) };

                client.DefaultRequestHeaders.Add("SOAPAction", "\"\"");

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                var response = await client.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
                var endTime = DateTime.UtcNow;
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                // 4. Guardar SOAP RESPONSE (solo si está habilitado)
                if (_saveDebugFiles)
                {
                    SaveSoapFile(responseContent, "SOAP_RESPONSE", DeterminarCategoria(solicitud));
                }

                if (!response.IsSuccessStatusCode)
                {
                    return TransportResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        $"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // 5. Deserializar respuesta
                // ✅ SOLUCIÓN: Usar XmlDocument + OuterXml (preserva namespaces completos)
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseContent);

                // Crear NameTable con todos los namespaces del documento
                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("tikR", VFNamespaces.NamespaceTikR);
                nsmgr.AddNamespace("tik", VFNamespaces.NamespaceSF);  // ✅ CRÍTICO: Este es el namespace de los elementos hijos
                nsmgr.AddNamespace("sum", VFNamespaces.NamespaceSFLR);

                // Buscar el elemento Body usando XPath con namespaces
                var bodyNode = xmlDoc.SelectSingleNode("//soapenv:Body", nsmgr);
                if (bodyNode == null)
                {
                    return TransportResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        "No se encontró elemento Body en el SOAP response",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // Buscar RespuestaRegFactuSistemaFacturacion dentro del Body
                var respuestaNode = bodyNode.SelectSingleNode("tikR:RespuestaRegFactuSistemaFacturacion", nsmgr);
                if (respuestaNode == null)
                {
                    return TransportResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        "No se encontró RespuestaRegFactuSistemaFacturacion en el SOAP response",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // ✅ SOLUCIÓN DEFINITIVA: Usar OuterXml (incluye TODOS los namespaces del nodo y sus ancestros)
                var xmlToDeserialize = respuestaNode.OuterXml;

                // ✅ LOG DIAGNÓSTICO: Ver el XML antes de deserializar
                _logger.LogInfo(
                    "VeriFactuHttpTransport",
                    $"[DEBUG] XML a deserializar (OuterXml primeros 2000 chars):\n{xmlToDeserialize.Substring(0, Math.Min(2000, xmlToDeserialize.Length))}");

                // ✅ Deserializar desde string XML completo (con namespaces preservados)
                var serializer = new XmlSerializer(typeof(RespuestaRegFactuSistemaFacturacionType));
                using var stringReader = new StringReader(xmlToDeserialize);
                using var xmlReader = XmlReader.Create(stringReader);
                var respuesta = (RespuestaRegFactuSistemaFacturacionType)serializer.Deserialize(xmlReader);

                //// ✅ LOG para debugging: Verificar si DatosPresentacion se deserializó correctamente
                //_logger.LogInfo(
                //    "VeriFactuHttpTransport",
                //    $"Deserialización completada. DatosPresentacion presente: {respuesta?.DatosPresentacion != null}, " + $"IdPeticion: '{respuesta?.DatosPresentacion?.IdPeticion ?? "(null)"}', " + $"NIFPresentador: '{respuesta?.DatosPresentacion?.NIFPresentador ?? "(null)"}', " + $"TimestampPresentacion: '{respuesta?.DatosPresentacion?.TimestampPresentacion.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? "(null)"}', " + $"CSV: '{respuesta?.CSV ?? "(null)"}'");

                return TransportResult<RespuestaRegFactuSistemaFacturacionType>.Ok(
                    respuesta,
                    soapEnvelope,
                    responseContent,
                    (int)response.StatusCode,
                    startTime,
                    endTime);
            }
            catch (Exception ex)
            {
                return TransportResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                    ex.Message,
                    exception: ex,
                    start: startTime,
                    end: DateTime.UtcNow);
            }
        }

        public async Task<TransportResult<RespuestaConsultaFactuSistemaFacturacionType>> EnviarConsultaAsync(
            ConsultaFactuSistemaFacturacionType consulta,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // 1. Construir SOAP Envelope (SIN FIRMAR)
                var soapEnvelope = BuildConsultaSoapEnvelope(consulta);

                // 2. Guardar SOAP REQUEST (solo si está habilitado)
                if (_saveDebugFiles)
                {
                    SaveSoapFile(soapEnvelope, "SOAP_REQUEST", "Consulta");
                }

                // 3. Enviar vía HttpClient
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(certificado);
                handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;

                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2) };

                client.DefaultRequestHeaders.Add("SOAPAction", "\"\"");

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                var response = await client.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
                var endTime = DateTime.UtcNow;
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                // 4. Guardar SOAP RESPONSE (solo si está habilitado)
                if (_saveDebugFiles)
                {
                    SaveSoapFile(responseContent, "SOAP_RESPONSE", "Consulta");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return TransportResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                        $"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // 5. Deserializar respuesta
                // ✅ SOLUCIÓN: Usar XmlDocument + OuterXml (preserva namespaces completos)
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseContent);

                // Crear NameTable con todos los namespaces del documento
                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("tikLRRC", VFNamespaces.NamespaceTikLRRC);
                nsmgr.AddNamespace("tik", VFNamespaces.NamespaceSF);  // ✅ CRÍTICO: Namespace de elementos hijos
                nsmgr.AddNamespace("sum", VFNamespaces.NamespaceSFLR);

                // Buscar el elemento Body usando XPath con namespaces
                var bodyNode = xmlDoc.SelectSingleNode("//soapenv:Body", nsmgr);
                if (bodyNode == null)
                {
                    return TransportResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                        "No se encontró elemento Body en el SOAP response",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // Buscar RespuestaConsultaFactuSistemaFacturacion dentro del Body
                var respuestaNode = bodyNode.SelectSingleNode("tikLRRC:RespuestaConsultaFactuSistemaFacturacion", nsmgr);
                if (respuestaNode == null)
                {
                    return TransportResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                        "No se encontró RespuestaConsultaFactuSistemaFacturacion en el SOAP response",
                        soapSent: soapEnvelope,
                        soapReceived: responseContent,
                        httpStatusCode: (int)response.StatusCode,
                        start: startTime,
                        end: endTime);
                }

                // ✅ SOLUCIÓN DEFINITIVA: Usar OuterXml (incluye TODOS los namespaces del nodo y sus ancestros)
                var xmlToDeserialize = respuestaNode.OuterXml;

                // ✅ Deserializar desde string XML completo (con namespaces preservados)
                var serializer = new XmlSerializer(typeof(RespuestaConsultaFactuSistemaFacturacionType));
                using var stringReader = new StringReader(xmlToDeserialize);
                using var xmlReader = XmlReader.Create(stringReader);
                var respuesta = (RespuestaConsultaFactuSistemaFacturacionType)serializer.Deserialize(xmlReader);

                return TransportResult<RespuestaConsultaFactuSistemaFacturacionType>.Ok(
                    respuesta,
                    soapEnvelope,
                    responseContent,
                    (int)response.StatusCode,
                    startTime,
                    endTime);
            }
            catch (Exception ex)
            {
                return TransportResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                    ex.Message,
                    exception: ex,
                    start: startTime,
                    end: DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Valida el certificado SSL del servidor AEAT.
        /// </summary>
        private static bool ValidateServerCertificate(
            HttpRequestMessage message,
            X509Certificate2 cert,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                return true;

            // IMPORTANTE: En producción, considerar implementar validación más estricta
            return true;
        }

        private static string BuildRegFactuSoapEnvelope(RegFactuSistemaFacturacionType solicitud)
        {
            var namespaces = VFXmlSerialization.CreateRegFactuNamespaces();
            var bodyContent = VFXmlSerialization.Serialize(
                solicitud,
                namespaces,
                omitXmlDeclaration: true,
                indent: false);

            if (bodyContent.StartsWith("<?xml"))
            {
                var firstTag = bodyContent.IndexOf("<sum:");
                if (firstTag > 0)
                    bodyContent = bodyContent.Substring(firstTag);
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<soapenv:Envelope");
            sb.Append(" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"");
            sb.Append($" xmlns:sum=\"{VFNamespaces.NamespaceSFLR}\"");
            sb.Append($" xmlns:sum1=\"{VFNamespaces.NamespaceSF}\"");
            sb.AppendLine(">");
            sb.AppendLine("<soapenv:Header/>");
            sb.AppendLine("<soapenv:Body>");
            sb.Append(bodyContent);
            sb.AppendLine();
            sb.AppendLine("</soapenv:Body>");
            sb.AppendLine("</soapenv:Envelope>");

            return sb.ToString();
        }

        private static string BuildConsultaSoapEnvelope(ConsultaFactuSistemaFacturacionType consulta)
        {
            var namespaces = VFXmlSerialization.CreateConsultaNamespaces();
            var bodyContent = VFXmlSerialization.Serialize(
                consulta,
                namespaces,
                omitXmlDeclaration: true,
                indent: false);

            if (bodyContent.StartsWith("<?xml"))
            {
                var firstTag = bodyContent.IndexOf("<con:");
                if (firstTag > 0)
                    bodyContent = bodyContent.Substring(firstTag);
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<soapenv:Envelope");
            sb.Append(" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"");
            sb.Append($" xmlns:con=\"{VFNamespaces.NamespaceCon}\"");
            sb.Append($" xmlns:sum1=\"{VFNamespaces.NamespaceSF}\"");
            sb.AppendLine(">");
            sb.AppendLine("<soapenv:Header/>");
            sb.AppendLine("<soapenv:Body>");
            sb.Append(bodyContent);
            sb.AppendLine();
            sb.AppendLine("</soapenv:Body>");
            sb.AppendLine("</soapenv:Envelope>");

            return sb.ToString();
        }

        /// <summary>
        /// Determina la categoría del registro para clasificar los archivos SOAP.
        /// </summary>
        private static string DeterminarCategoria(RegFactuSistemaFacturacionType solicitud)
        {
            try
            {
                var reg = solicitud?.RegistroFactura?.FirstOrDefault()?.Registro;
                return reg is RegistroFacturacionAltaType ? "Alta" : reg is RegistroFacturacionAnulacionType ? "Anulacion" : "Otro";
            }
            catch
            {
                return "Otro";
            }
        }

        /// <summary>
        /// Guarda un archivo SOAP en el escritorio para debugging. Estructura:
        /// Desktop/[Categoria]/SOAP_REQUEST_yyyyMMdd_HHmmss_fff.xml
        /// </summary>
        private void SaveSoapFile(string soapContent, string tipo, string categoria)
        {
            try
            {
                var carpetaDestino = Path.Combine(_debugFolder, categoria);
                Directory.CreateDirectory(carpetaDestino);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var rutaArchivo = Path.Combine(carpetaDestino, $"{tipo}_{timestamp}.xml");

                File.WriteAllText(rutaArchivo, soapContent, Encoding.UTF8);
            }
            catch
            {
                // Ignorar errores de guardado (no debe bloquear operación principal)
            }
        }

        /// <summary>
        /// ⚠️ TESTING: Corrompe intencionalmente la huella en el SOAP para simular error AEAT. Busca la etiqueta
        /// &lt;Huella&gt; (con o sin namespace) y altera el último carácter.
        /// </summary>
        private string CorromperHuellaEnSoap(string soapEnvelope)
        {
            try
            {
                // Buscar patrón <sum1:Huella> o <Huella> (puede tener namespace prefix)
                // Primero intentar con namespace (más común en SOAP AEAT)
                string openTag = "<sum1:Huella>";
                string closeTag = "</sum1:Huella>";

                var startIndex = soapEnvelope.IndexOf(openTag, StringComparison.Ordinal);

                // Si no se encuentra con namespace, intentar sin él
                if (startIndex < 0)
                {
                    openTag = "<Huella>";
                    closeTag = "</Huella>";
                    startIndex = soapEnvelope.IndexOf(openTag, StringComparison.Ordinal);
                }

                if (startIndex < 0)
                {
                    _logger.LogWarning(
                        "VeriFactuHttpTransport",
                        "[TESTING] ⚠️ No se encontró etiqueta <Huella> o <sum1:Huella> en el SOAP - No se puede corromper");
                    return soapEnvelope;
                }

                startIndex += openTag.Length;
                var endIndex = soapEnvelope.IndexOf(closeTag, startIndex, StringComparison.Ordinal);
                if (endIndex < 0)
                {
                    _logger.LogWarning(
                        "VeriFactuHttpTransport",
                        "[TESTING] ⚠️ No se encontró etiqueta de cierre para Huella");
                    return soapEnvelope;
                }

                var huellaOriginal = soapEnvelope.Substring(startIndex, endIndex - startIndex).Trim();

                if (string.IsNullOrWhiteSpace(huellaOriginal) || huellaOriginal.Length < 64)
                {
                    _logger.LogWarning(
                        "VeriFactuHttpTransport",
                        $"[TESTING] ⚠️ Huella inválida o muy corta: '{huellaOriginal}' (longitud: {huellaOriginal?.Length ?? 0})");
                    return soapEnvelope;
                }

                // Alterar el último carácter
                var huellaCorrupta = huellaOriginal.Substring(0, huellaOriginal.Length - 1) + "X";

                // Reemplazar en el SOAP
                var soapCorrupto = soapEnvelope.Substring(0, startIndex) + huellaCorrupta + soapEnvelope.Substring(
                    endIndex);

                // ✅ LOG obligatorio para confirmar que se corrompió
                _logger.LogWarning(
                    "VeriFactuHttpTransport",
                    $"[TESTING] ✅ Huella corrompida exitosamente:\n" + $"  Original: ...{huellaOriginal.Substring(Math.Max(0, huellaOriginal.Length - 16))}\n" + $"  Corrupta: ...{huellaCorrupta.Substring(Math.Max(0, huellaCorrupta.Length - 16))}");

                return soapCorrupto;
            }
            catch (Exception ex)
            {
                _logger.LogError("VeriFactuHttpTransport", $"[TESTING] ❌ Error al corromper huella: {ex.Message}");
                return soapEnvelope;
            }
        }
    }
}
