using System.Security.Cryptography.X509Certificates;
using Gesdata.VF.Contracts.Types;

namespace Gesdata.VF.Core.Services.Transport
{
    /// <summary>
    /// Abstracción del transporte de comunicación con AEAT.
    /// Permite implementaciones alternativas (HTTP directo, WCF, mock para testing).
    /// </summary>
    public interface IAeatTransport
    {
        /// <summary>
        /// Envía un registro de facturación a AEAT.
        /// </summary>
        /// <param name="solicitud">Solicitud de registro (firmada previamente).</param>
        /// <param name="endpoint">Endpoint de AEAT (producción o preproducción).</param>
        /// <param name="certificado">Certificado X509 para autenticación SSL.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado del transporte con respuesta SOAP y metadatos.</returns>
        Task<TransportResult<RespuestaRegFactuSistemaFacturacionType>> EnviarRegistroAsync(
            RegFactuSistemaFacturacionType solicitud,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía una consulta de registros a AEAT (sin firma digital).
        /// </summary>
        /// <param name="consulta">Solicitud de consulta (NO firmada).</param>
        /// <param name="endpoint">Endpoint de AEAT.</param>
        /// <param name="certificado">Certificado X509 para autenticación SSL.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado del transporte con respuesta SOAP y metadatos.</returns>
        Task<TransportResult<RespuestaConsultaFactuSistemaFacturacionType>> EnviarConsultaAsync(
            ConsultaFactuSistemaFacturacionType consulta,
            Uri endpoint,
            X509Certificate2 certificado,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Resultado de una operación de transporte SOAP a AEAT.
    /// </summary>
    /// <typeparam name="TResponse">Tipo de respuesta deserializada (RespuestaRegFactu o RespuestaConsulta).</typeparam>
    public sealed class TransportResult<TResponse> where TResponse : class
    {
        /// <summary>
        /// Indica si la operación de transporte fue exitosa (HTTP 200 + SOAP válido).
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Respuesta deserializada de AEAT (null si hay error de transporte).
        /// </summary>
        public TResponse Response { get; init; }

        /// <summary>
        /// XML SOAP completo enviado a AEAT (para auditoría/persistencia).
        /// </summary>
        public string SoapEnvelopeSent { get; init; }

        /// <summary>
        /// XML SOAP completo recibido de AEAT (para auditoría/persistencia).
        /// </summary>
        public string SoapEnvelopeReceived { get; init; }

        /// <summary>
        /// Código de estado HTTP (200, 500, etc.).
        /// </summary>
        public int? HttpStatusCode { get; init; }

        /// <summary>
        /// Mensaje de error (solo si Success = false).
        /// </summary>
        public string ErrorMessage { get; init; }

        /// <summary>
        /// Excepción capturada (solo si Success = false).
        /// </summary>
        public Exception Exception { get; init; }

        /// <summary>
        /// Timestamp de inicio del envío.
        /// </summary>
        public DateTime StartTimestamp { get; init; }

        /// <summary>
        /// Timestamp de finalización (recepción de respuesta o error).
        /// </summary>
        public DateTime EndTimestamp { get; init; }

        /// <summary>
        /// Duración total de la operación.
        /// </summary>
        public TimeSpan Duration => EndTimestamp - StartTimestamp;

        public static TransportResult<TResponse> Ok(
            TResponse response,
            string soapSent,
            string soapReceived,
            int httpStatusCode,
            DateTime start,
            DateTime end)
        {
            return new TransportResult<TResponse>
            {
                Success = true,
                Response = response,
                SoapEnvelopeSent = soapSent,
                SoapEnvelopeReceived = soapReceived,
                HttpStatusCode = httpStatusCode,
                StartTimestamp = start,
                EndTimestamp = end
            };
        }

        public static TransportResult<TResponse> Fail(
            string errorMessage,
            Exception exception = null,
            string soapSent = null,
            string soapReceived = null,
            int? httpStatusCode = null,
            DateTime? start = null,
            DateTime? end = null)
        {
            return new TransportResult<TResponse>
            {
                Success = false,
                ErrorMessage = errorMessage,
                Exception = exception,
                SoapEnvelopeSent = soapSent,
                SoapEnvelopeReceived = soapReceived,
                HttpStatusCode = httpStatusCode,
                StartTimestamp = start ?? DateTime.UtcNow,
                EndTimestamp = end ?? DateTime.UtcNow
            };
        }
    }
}
