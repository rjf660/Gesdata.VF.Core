using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Gesdata.VF.Contracts.Types;
using Gesdata.VF.Core.Exceptions;

namespace Gesdata.VF.Core.Errors
{
    /// <summary>
    /// Analizador unificado de errores AEAT.
    /// ✅ CONSOLIDADO: Combina clasificación, extracción y análisis de errores.
    /// 
    /// Responsabilidades:
    /// - Clasificar errores (transitorio vs permanente)
    /// - Extraer códigos de error desde respuestas/SOAP/texto
    /// - Analizar respuestas completas y generar resúmenes
    /// - Generar mensajes amigables para usuarios
    /// </summary>
    public static class AeatErrorAnalyzer
    {
        #region Clasificación de Errores (antes AeatErrorClassifier)

        /// <summary>
        /// Códigos de error AEAT que indican problemas transitorios del servidor.
        /// Estos errores suelen resolverse reintentando después de unos minutos.
        /// </summary>
        public static readonly HashSet<string> CodigosErroresTransitorios = new(StringComparer.OrdinalIgnoreCase)
        {
            "20009", // Error interno en el servidor
            "3500",  // Error técnico de base de datos
            "3501",  // Error técnico de base de datos
            "4128",  // Error técnico en la recuperación del valor del Gestor de Tablas
            "4134",  // Servicio no activo
            "4141",  // Acceso suspendido temporalmente
        };

        /// <summary>
        /// Patrones de texto en faultstring que indican errores transitorios.
        /// </summary>
        private static readonly string[] PatronesErrorTransitorio =
        {
            "error interno",
            "error técnico",
            "servicio no activo",
            "temporalmente",
            "timeout",
            "time out",
            "connection",
            "network",
        };

        /// <summary>
        /// Determina si un error AEAT es transitorio (puede resolverse reintentando).
        /// </summary>
        /// <param name="codigoError">Código de error AEAT (ej: "20009")</param>
        /// <param name="mensajeError">Mensaje de error completo</param>
        /// <returns>True si el error es transitorio y se recomienda reintentar</returns>
        public static bool EsErrorTransitorio(string codigoError, string mensajeError = null)
        {
            // 1. Verificar por código
            if (!string.IsNullOrWhiteSpace(codigoError))
            {
                var codigo = codigoError.Trim();
                if (CodigosErroresTransitorios.Contains(codigo))
                    return true;
            }

            // 2. Verificar por patrón de texto en el mensaje
            if (!string.IsNullOrWhiteSpace(mensajeError))
            {
                var mensajeLower = mensajeError.ToLowerInvariant();
                foreach (var patron in PatronesErrorTransitorio)
                {
                    if (mensajeLower.Contains(patron))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Lanza la excepción apropiada según el tipo de error.
        /// Esto facilita el uso con políticas de Polly.
        /// </summary>
        /// <param name="codigoError">Código de error AEAT</param>
        /// <param name="mensajeError">Mensaje de error completo</param>
        /// <param name="innerException">Excepción original (opcional)</param>
        /// <exception cref="AeatTransientException">Si el error es transitorio</exception>
        /// <exception cref="AeatPermanentException">Si el error es permanente</exception>
        public static void LanzarExcepcion(string codigoError, string mensajeError, Exception innerException = null)
        {
            var esTransitorio = EsErrorTransitorio(codigoError, mensajeError);

            if (esTransitorio)
            {
                var tiempoEspera = ObtenerTiempoEsperaRecomendado(codigoError);
                throw new AeatTransientException(
                    codigoError ?? "DESCONOCIDO",
                    "Error transitorio AEAT",
                    GenerarMensajeUsuario(codigoError, mensajeError),
                    innerException)
                {
                    TiempoEsperaRecomendado = tiempoEspera
                };
            }
            else
            {
                throw new AeatPermanentException(
                    codigoError ?? "DESCONOCIDO",
                    "Error permanente AEAT",
                    GenerarMensajeUsuario(codigoError, mensajeError),
                    innerException);
            }
        }

        /// <summary>
        /// Obtiene el tiempo de espera recomendado antes de reintentar.
        /// </summary>
        private static TimeSpan ObtenerTiempoEsperaRecomendado(string codigoError)
        {
            return codigoError switch
            {
                "4141" => TimeSpan.FromMinutes(5), // Acceso suspendido temporalmente
                "4134" => TimeSpan.FromMinutes(2), // Servicio no activo
                _ => TimeSpan.FromSeconds(30)      // Default para otros errores transitorios
            };
        }

        #endregion

        #region Extracción de Errores (antes AeatErrorExtractor)

        /// <summary>
        /// Extrae TODOS los errores desde una respuesta de registro (Alta/Anulación).
        /// </summary>
        /// <param name="respuesta">Respuesta de AEAT tras registro de facturas</param>
        /// <returns>Lista de tuplas (Código, Descripción) por cada error encontrado</returns>
        public static IReadOnlyList<ErrorAeat> ExtraerErroresRegistro(
            RespuestaRegFactuSistemaFacturacionType respuesta)
        {
            var errores = new List<ErrorAeat>();

            if (respuesta?.RespuestaLinea == null)
                return errores;

            foreach (var linea in respuesta.RespuestaLinea)
            {
                var codigo = linea?.CodigoErrorRegistro?.ToString();
                var descripcion = linea?.DescripcionErrorRegistro ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(codigo) && codigo != "0")
                {
                    errores.Add(new ErrorAeat
                    {
                        Codigo = codigo,
                        Descripcion = descripcion,
                        EsTransitorio = EsErrorTransitorio(codigo, descripcion)
                    });
                }
            }

            return errores;
        }

        /// <summary>
        /// Extrae errores de una línea específica de respuesta.
        /// </summary>
        public static ErrorAeat ExtraerErrorLinea(RespuestaExpedidaType linea)
        {
            if (linea == null)
                return null;

            var codigo = linea.CodigoErrorRegistro?.ToString();
            var descripcion = linea.DescripcionErrorRegistro ?? string.Empty;

            // Código 0 = sin errores
            return codigo == "0" || string.IsNullOrWhiteSpace(codigo)
                ? null
                : new ErrorAeat
                {
                    Codigo = codigo,
                    Descripcion = descripcion,
                    EsTransitorio = EsErrorTransitorio(codigo, descripcion)
                };
        }

        /// <summary>
        /// Extrae errores desde una respuesta de consulta.
        /// </summary>
        public static IReadOnlyList<ErrorAeat> ExtraerErroresConsulta(
            RespuestaConsultaFactuSistemaFacturacionType respuesta)
        {
            var errores = new List<ErrorAeat>();

            if (respuesta?.RegistroRespuestaConsultaFactuSistemaFacturacion == null)
                return errores;

            foreach (var registro in respuesta.RegistroRespuestaConsultaFactuSistemaFacturacion)
            {
                if (registro?.EstadoRegistro != null)
                {
                    var codigo = registro.EstadoRegistro.CodigoErrorRegistro.ToString();
                    var descripcion = registro.EstadoRegistro.DescripcionErrorRegistro ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(codigo) && codigo != "0")
                    {
                        errores.Add(new ErrorAeat
                        {
                            Codigo = codigo,
                            Descripcion = descripcion,
                            EsTransitorio = EsErrorTransitorio(codigo, descripcion)
                        });
                    }
                }
            }

            return errores;
        }

        /// <summary>
        /// Extrae código de error desde un SOAP faultstring.
        /// 
        /// Formatos soportados:
        /// - "Codigo[XXXX]. Descripción"
        /// - "XXXX - Descripción"
        /// - "Error XXXX: Descripción"
        /// </summary>
        public static string ExtraerCodigoSoapFault(string faultString)
        {
            if (string.IsNullOrWhiteSpace(faultString))
                return null;

            // 1. Patrón: Codigo[XXXX]
            var match = Regex.Match(faultString, @"Codigo\[(\d+)\]", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;

            // 2. Patrón: XXXX - o XXXX: al inicio
            match = Regex.Match(faultString, @"^(\d{4,5})\s*[-:]");
            if (match.Success)
                return match.Groups[1].Value;

            // 3. Patrón: Error XXXX:
            match = Regex.Match(faultString, @"Error\s+(\d{4,5})", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;

            // 4. Buscar cualquier número de 4-5 dígitos en el texto
            match = Regex.Match(faultString, @"\b(\d{4,5})\b");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Extrae el faultstring completo desde XML SOAP.
        /// Devuelve el contenido completo del nodo faultstring (ej: "Codigo[401].Certificado revocado")
        /// </summary>
        public static string ExtraerFaultStringSoap(string soapXml)
        {
            if (string.IsNullOrWhiteSpace(soapXml))
                return null;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(soapXml);

                // Buscar faultstring en el SOAP Fault
                var faultStringNode = xmlDoc.SelectSingleNode("//faultstring");
                if (faultStringNode != null)
                {
                    return faultStringNode.InnerText?.Trim();
                }

                // Fallback: buscar en otros posibles nodos de error
                var errorNodes = new[]
                {
                    "//DescripcionErrorRegistro",
                    "//DescripcionError",
                    "//ErrorMessage"
                };

                foreach (var xpath in errorNodes)
                {
                    var node = xmlDoc.SelectSingleNode(xpath);
                    if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        return node.InnerText.Trim();
                    }
                }
            }
            catch
            {
                // Si falla el parseo XML, intentar con regex
                var match = Regex.Match(soapXml, @"<faultstring>(.+?)</faultstring>", RegexOptions.Singleline);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// Extrae códigos de error desde XML SOAP completo.
        /// </summary>
        public static IReadOnlyList<string> ExtraerCodigosSoapXml(string soapXml)
        {
            var codigos = new List<string>();

            if (string.IsNullOrWhiteSpace(soapXml))
                return codigos;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(soapXml);

                // Buscar elementos comunes de error
                var xpaths = new[]
                {
                    "//CodigoErrorRegistro",
                    "//CodigoError",
                    "//faultcode",
                    "//faultstring"
                };

                foreach (var xpath in xpaths)
                {
                    var nodes = xmlDoc.SelectNodes(xpath);
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            var valor = node.InnerText?.Trim();
                            if (!string.IsNullOrWhiteSpace(valor))
                            {
                                // Si es numérico directamente, agregarlo
                                if (int.TryParse(valor, out var codigo) && codigo > 0)
                                {
                                    codigos.Add(valor);
                                }
                                else
                                {
                                    // Intentar extraer código del texto
                                    var codigoExtraido = ExtraerCodigoSoapFault(valor);
                                    if (codigoExtraido != null)
                                    {
                                        codigos.Add(codigoExtraido);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si falla el parseo XML, intentar con regex
                var matches = Regex.Matches(soapXml, @"<(?:CodigoErrorRegistro|CodigoError)>(\d+)</");
                foreach (Match match in matches)
                {
                    codigos.Add(match.Groups[1].Value);
                }
            }

            return codigos.Distinct().ToList();
        }

        /// <summary>
        /// Extrae códigos numéricos de 3-6 dígitos desde texto libre.
        /// </summary>
        public static IReadOnlyList<string> ExtraerCodigosTextoLibre(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return Array.Empty<string>();

            var matches = Regex.Matches(texto, @"(?<![0-9])([0-9]{3,6})(?![0-9])");
            return matches.Select(m => m.Value).Distinct().ToArray();
        }

        #endregion

        #region Análisis Completo (consolidado)

        /// <summary>
        /// Analiza completamente una respuesta de registro y devuelve resumen de errores.
        /// ✅ Método "todo en uno" para usar desde Application Service.
        /// </summary>
        public static AnalisisErrores AnalizarRespuestaRegistro(
            RespuestaRegFactuSistemaFacturacionType respuesta)
        {
            var errores = ExtraerErroresRegistro(respuesta);

            var codigosTransitorios = errores
                .Where(e => e.EsTransitorio)
                .Select(e => e.Codigo)
                .ToList();

            var codigosPermanentes = errores
                .Where(e => !e.EsTransitorio)
                .Select(e => e.Codigo)
                .ToList();

            var descripcionesResumidas = errores
                .Select(e =>
                {
                    string desc;
                    return AeatErrorCatalog.Instance.TryGetMessage(e.Codigo, out desc) ? desc : e.Descripcion;
                })
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

            return new AnalisisErrores
            {
                TotalErrores = errores.Count,
                Errores = errores,
                CodigosTransitorios = codigosTransitorios,
                CodigosPermanentes = codigosPermanentes,
                DescripcionesResumidas = descripcionesResumidas,
                ResumenCompleto = GenerarResumenCompleto(errores)
            };
        }

        /// <summary>
        /// Genera un resumen textual legible de los errores.
        /// </summary>
        private static string GenerarResumenCompleto(IReadOnlyList<ErrorAeat> errores)
        {
            if (errores.Count == 0)
                return "Sin errores";

            var sb = new StringBuilder();

            foreach (var error in errores)
            {
                string catalogoDesc;
                var tieneCatalogo = AeatErrorCatalog.Instance.TryGetMessage(error.Codigo, out catalogoDesc);

                sb.AppendLine($"[{error.Codigo}] {(tieneCatalogo ? catalogoDesc : error.Descripcion)}");

                if (tieneCatalogo &&
                    !string.IsNullOrWhiteSpace(error.Descripcion) &&
                    catalogoDesc != error.Descripcion)
                {
                    sb.AppendLine($"        AEAT: {error.Descripcion}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Genera un mensaje amigable para el usuario según el tipo de error.
        /// </summary>
        /// <param name="codigoError">Código de error AEAT</param>
        /// <param name="mensajeError">Mensaje de error original</param>
        /// <returns>Mensaje formateado para mostrar al usuario</returns>
        public static string GenerarMensajeUsuario(string codigoError, string mensajeError)
        {
            var esTransitorio = EsErrorTransitorio(codigoError, mensajeError);

            return esTransitorio
                ? $"⚠️ Error temporal del servicio de AEAT\n\n" +
                       $"El servidor de AEAT está experimentando problemas temporales.\n\n" +
                       $"Código: {codigoError ?? "Desconocido"}\n" +
                       $"Mensaje: {mensajeError}\n\n" +
                       $"💡 Recomendación: Espere unos minutos e intente de nuevo.\n" +
                       $"Si el problema persiste, contacte con soporte."
                : $"❌ Error en la consulta a AEAT\n\n" +
                       $"Código: {codigoError ?? "Desconocido"}\n" +
                       $"Mensaje: {mensajeError}\n\n" +
                       $"Por favor, revise los datos enviados o contacte con soporte.";
        }

        #endregion
    }

    /// <summary>
    /// Representa un error individual de AEAT.
    /// </summary>
    public sealed class ErrorAeat
    {
        public string Codigo { get; init; }
        public string Descripcion { get; init; }
        public bool EsTransitorio { get; init; }

        /// <summary>
        /// Descripción desde el catálogo interno (más detallada).
        /// </summary>
        public string DescripcionCatalogo
        {
            get
            {
                string desc;
                return AeatErrorCatalog.Instance.TryGetMessage(Codigo, out desc) ? desc : null;
            }
        }
    }

    /// <summary>
    /// Resultado del análisis completo de errores de una respuesta AEAT.
    /// </summary>
    public sealed class AnalisisErrores
    {
        public int TotalErrores { get; init; }

        public IReadOnlyList<ErrorAeat> Errores { get; init; }

        public IReadOnlyList<string> CodigosTransitorios { get; init; }

        public IReadOnlyList<string> CodigosPermanentes { get; init; }

        public IReadOnlyList<string> DescripcionesResumidas { get; init; }

        public string ResumenCompleto { get; init; }

        public bool TieneErroresTransitorios => CodigosTransitorios?.Count > 0;

        public bool TieneErroresPermanentes => CodigosPermanentes?.Count > 0;

        public bool SinErrores => TotalErrores == 0;
    }
}