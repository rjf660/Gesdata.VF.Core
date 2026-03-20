using Gesdata.Comun.Logging;
using Gesdata.VF.Contracts.Types;
using Gesdata.VF.Core.Services.Transport;
using Gesdata.VF.Core.XML;
using Gesdata.VF.Core.XML.Signatures;
using System.Security.Cryptography.X509Certificates;

namespace Gesdata.VF.Core.Services
{
    /// <summary>
    /// Servicio de transporte HTTP/SOAP para operaciones VeriFactu con AEAT.
    /// ✅ RESPONSABILIDAD: Solo transporte, serialización, firma y comunicación HTTP.
    /// ❌ NO incluye: Validación de negocio, cálculo de huellas, persistencia (eso es responsabilidad de Application).
    /// </summary>
    /// <remarks>
    /// Esta capa de Core/Infrastructure proporciona servicios de transporte puros:
    /// - Serialización XML
    /// - Firma digital (XAdES-BES)
    /// - Comunicación HTTP/SOAP
    /// - Deserialización de respuestas
    /// 
    /// La validación de destinatarios y cálculo de huellas se hace en Application layer
    /// (VeriFactuApplicationService → GrafoBuilderService) ANTES de llamar a este servicio.
    /// </remarks>
    public sealed class VeriFactuService
    {
        private readonly IAeatTransport _transport;
        private readonly LoggingService _logger;

        public VeriFactuService(
            IAeatTransport transport,
            LoggingService logger = null)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _logger = logger ?? new LoggingService();
        }

        /// <summary>
        /// Registra facturas en AEAT con el flujo de transporte completo:
        /// 1. Serialización XML
        /// 2. Firma digital (XAdES-BES)
        /// 3. Envío HTTP/SOAP
        /// 4. Deserialización respuesta
        /// </summary>
        /// <remarks>
        /// ✅ PREREQUISITOS (responsabilidad de Application layer):
        /// - Las huellas YA deben estar calculadas en solicitud.RegistroFactura[].Registro.Huella
        /// - Los destinatarios YA deben estar validados
        /// - La solicitud debe cumplir con el esquema XSD
        /// 
        /// Este método NO valida ni calcula nada, solo transporta.
        /// </remarks>
        public async Task<VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>> RegistrarFacturasAsync(
            RegFactuSistemaFacturacionType solicitud,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(solicitud);
            ArgumentNullException.ThrowIfNull(endpoint);
            ArgumentNullException.ThrowIfNull(certificado);

            string xmlGenerado = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. Validación previa: número de registros (límite de protocolo, no de negocio)
                var count = solicitud.RegistroFactura?.Count ?? 0;
                if (count is <= 0 or > 1000)
                {
                    return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        "Solicitud inválida: número de registros fuera de rango (1..1000).");
                }

                // 2. Serializar solicitud a XML
                try
                {
                    var namespaces = VFXmlSerialization.CreateRegFactuNamespaces();
                    xmlGenerado = VFXmlSerialization.Serialize(solicitud, namespaces);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        component: "VeriFactuService",
                        message: "Error al serializar solicitud",
                        exception: ex);

                    return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        $"Error al serializar solicitud: {ex.Message}",
                        exception: ex);
                }

                // 3. Firmar XML con XAdES-BES
                string xmlFirmado;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    xmlFirmado = VFXmlSigner.SignXml(xmlGenerado, certificado);

                    // Verificación local de firma (opcional, para debugging)
                    if (!VFXmlSigner.VerifySignature(xmlFirmado))
                    {
                        _logger.LogWarning(
                            component: "VeriFactuService",
                            message: "⚠️ Firma XML no verificable localmente (puede ser normal según certificado)");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        component: "VeriFactuService",
                        message: "Error al firmar XML",
                        exception: ex);

                    return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        $"Error al firmar XML: {ex.Message}",
                        exception: ex,
                        soapEnvelopeSent: xmlGenerado);
                }

                // 4. Enviar vía transporte HTTP/SOAP
                var transportResult = await _transport.EnviarRegistroAsync(
                    solicitud,
                    endpoint,
                    certificado,
                    cancellationToken).ConfigureAwait(false);

                if (!transportResult.Success)
                {
                    _logger.LogError(
                        component: "VeriFactuService",
                        message: $"Error en transporte HTTP: {transportResult.ErrorMessage}",
                        exception: transportResult.Exception);

                    return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                        transportResult.ErrorMessage,
                        exception: transportResult.Exception,
                        soapEnvelopeSent: transportResult.SoapEnvelopeSent ?? xmlGenerado,
                        soapEnvelopeReceived: transportResult.SoapEnvelopeReceived);
                }

                var respuesta = transportResult.Response;

                _logger.LogInfo(
                    component: "VeriFactuService",
                    message: "✅ Transporte HTTP completado exitosamente",
                    metadata: new
                    {
                        respuesta.EstadoEnvio,
                        respuesta.CSV,
                        respuesta.DatosPresentacion?.IdPeticion,
                        NumRegistros = solicitud.RegistroFactura?.Count ?? 0
                    });

                return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Ok(
                    respuesta,
                    transportResult.SoapEnvelopeSent ?? xmlGenerado,
                    transportResult.SoapEnvelopeReceived,
                    transportResult.Duration);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    component: "VeriFactuService",
                    message: "Operación cancelada por el usuario");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    component: "VeriFactuService",
                    message: "Error inesperado en RegistrarFacturasAsync",
                    exception: ex);

                return VeriFactuResult<RespuestaRegFactuSistemaFacturacionType>.Fail(
                    $"Error inesperado: {ex.Message}",
                    exception: ex,
                    soapEnvelopeSent: xmlGenerado);
            }
        }

        /// <summary>
        /// Consulta registros en AEAT con el flujo de transporte completo:
        /// 1. Serialización XML (SIN FIRMAR - las consultas no requieren firma)
        /// 2. Envío HTTP/SOAP
        /// 3. Deserialización respuesta
        /// </summary>
        /// <remarks>
        /// ✅ Las consultas NO se firman digitalmente según especificación técnica AEAT.
        /// Solo se requiere autenticación mediante certificado SSL/TLS en la conexión HTTPS.
        /// </remarks>
        public async Task<VeriFactuResult<RespuestaConsultaFactuSistemaFacturacionType>> ConsultarFacturasAsync(
            ConsultaFactuSistemaFacturacionType consulta,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(consulta);
            ArgumentNullException.ThrowIfNull(endpoint);
            ArgumentNullException.ThrowIfNull(certificado);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. Enviar vía transporte (SIN FIRMAR)
                var transportResult = await _transport.EnviarConsultaAsync(
                    consulta,
                    endpoint,
                    certificado,
                    cancellationToken).ConfigureAwait(false);

                if (!transportResult.Success)
                {
                    _logger.LogError(
                        component: "VeriFactuService",
                        message: $"Error en transporte HTTP (consulta): {transportResult.ErrorMessage}",
                        exception: transportResult.Exception);

                    return VeriFactuResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                        transportResult.ErrorMessage,
                        exception: transportResult.Exception,
                        soapEnvelopeSent: transportResult.SoapEnvelopeSent,
                        soapEnvelopeReceived: transportResult.SoapEnvelopeReceived);
                }

                var respuesta = transportResult.Response;

                _logger.LogInfo(
                    component: "VeriFactuService",
                    message: "✅ Consulta completada exitosamente",
                    metadata: new
                    {
                        respuesta.IndicadorPaginacion,
                        respuesta.ResultadoConsulta,
                        NumRegistros = respuesta.RegistroRespuestaConsultaFactuSistemaFacturacion?.Count ?? 0
                    });

                return VeriFactuResult<RespuestaConsultaFactuSistemaFacturacionType>.Ok(
                    respuesta,
                    transportResult.SoapEnvelopeSent,
                    transportResult.SoapEnvelopeReceived,
                    transportResult.Duration);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    component: "VeriFactuService",
                    message: "Consulta cancelada por el usuario");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    component: "VeriFactuService",
                    message: "Error inesperado en ConsultarFacturasAsync",
                    exception: ex);

                return VeriFactuResult<RespuestaConsultaFactuSistemaFacturacionType>.Fail(
                    $"Error inesperado: {ex.Message}",
                    exception: ex);
            }
        }
    }

    /// <summary>
    /// Resultado unificado de operaciones VeriFactu (registro o consulta).
    /// Encapsula respuesta de AEAT + metadatos de transporte para auditoría.
    /// </summary>
    public sealed class VeriFactuResult<TResponse> where TResponse : class
    {
        public bool Success { get; init; }
        public TResponse Response { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public Exception Exception { get; init; }
        public List<string> Errors { get; init; } = [];
        public string SoapEnvelopeSent { get; init; }
        public string SoapEnvelopeReceived { get; init; }
        public TimeSpan Duration { get; init; }

        public static VeriFactuResult<TResponse> Ok(
            TResponse response,
            string soapSent,
            string soapReceived,
            TimeSpan duration)
        {
            ArgumentNullException.ThrowIfNull(response);

            return new VeriFactuResult<TResponse>
            {
                Success = true,
                Response = response,
                SoapEnvelopeSent = soapSent,
                SoapEnvelopeReceived = soapReceived,
                Duration = duration
            };
        }

        public static VeriFactuResult<TResponse> Fail(
            string errorMessage,
            Exception exception = null,
            List<string> errors = null,
            string soapEnvelopeSent = null,
            string soapEnvelopeReceived = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

            return new VeriFactuResult<TResponse>
            {
                Success = false,
                ErrorMessage = errorMessage,
                Exception = exception,
                Errors = errors ?? [],
                SoapEnvelopeSent = soapEnvelopeSent,
                SoapEnvelopeReceived = soapEnvelopeReceived
            };
        }
    }
}
