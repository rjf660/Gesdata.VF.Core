using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace Gesdata.VF.Core.Diagnostics
{
    /// <summary>
    /// Inspector de mensajes SOAP para capturar el XML real enviado por WCF (con SOAP Envelope, firma, etc.)
    /// Útil para diagnóstico y auditoría de comunicaciones con AEAT.
    /// </summary>
    public class SoapMessageCaptureInspector : IClientMessageInspector, IEndpointBehavior
    {
        private readonly Action<string, string> onMessageCaptured;

        /// <summary>
        /// Crea un nuevo inspector con un callback para procesar los mensajes capturados.
        /// </summary>
        /// <param name="onMessageCaptured">Callback (requestXml, responseXml) que se invoca con cada mensaje.</param>
        public SoapMessageCaptureInspector(Action<string, string> onMessageCaptured)
        {
            this.onMessageCaptured = onMessageCaptured;
        }

        #region IClientMessageInspector

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
                // Crear una copia del mensaje ANTES de leerlo
                var buffer = request.CreateBufferedCopy(int.MaxValue);

                // Capturar el XML de la copia
                var messageCopy = buffer.CreateMessage();
                var requestXml = MessageToString(messageCopy);

                // Reemplazar el mensaje original con otra copia del buffer
                request = buffer.CreateMessage();

                // Notificar al callback con el XML capturado
                onMessageCaptured?.Invoke(requestXml, null);
            }
            catch
            {
                // Silenciar errores en producción
            }

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            try
            {
                // Crear una copia del mensaje ANTERIOR a leerlo
                var buffer = reply.CreateBufferedCopy(int.MaxValue);

                // Capturar el XML de la copia
                var messageCopy = buffer.CreateMessage();
                var responseXml = MessageToString(messageCopy);

                // Reemplazar el mensaje original con otra copia del buffer
                reply = buffer.CreateMessage();

                // Notificar al callback con el XML de respuesta
                onMessageCaptured?.Invoke(null, responseXml);
            }
            catch
            {
                // Silenciar errores en producción
            }
        }

        #endregion

        #region IEndpointBehavior

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // No necesitamos modificar parámetros de binding
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // Agregar este inspector a la lista de inspectores del cliente
            clientRuntime.ClientMessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // No aplicamos comportamiento de dispatch (esto es para servicios, no clientes)
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No necesitamos validar nada
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convierte un mensaje WCF a string XML.
        /// </summary>
        private static string MessageToString(Message message)
        {
            if (message == null)
                return string.Empty;

            var sb = new StringBuilder(4096);

            using (var sw = new StringWriter(sb))
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            }))
            {
                // Escribir el mensaje directamente (ya es una copia del buffer)
                message.WriteMessage(xw);
                xw.Flush();
            }

            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Factory method para crear un inspector con guardado automático en archivos.
        /// </summary>
        /// <param name="outputDirectory">Directorio donde guardar los XML capturados.</param>
        /// <param name="filePrefix">Prefijo para los nombres de archivo.</param>
        public static SoapMessageCaptureInspector CreateWithFileOutput(string outputDirectory, string filePrefix = "SOAP")
        {
            return new SoapMessageCaptureInspector((request, response) =>
            {
                try
                {
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);

                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

                    if (!string.IsNullOrEmpty(request))
                    {
                        var requestPath = Path.Combine(outputDirectory, $"{filePrefix}_{timestamp}_Request.xml");
                        File.WriteAllText(requestPath, request, Encoding.UTF8);
                    }

                    if (!string.IsNullOrEmpty(response))
                    {
                        var responsePath = Path.Combine(outputDirectory, $"{filePrefix}_{timestamp}_Response.xml");
                        File.WriteAllText(responsePath, response, Encoding.UTF8);
                    }
                }
                catch
                {
                    // Silenciar errores en producción
                }
            });
        }
    }
}
