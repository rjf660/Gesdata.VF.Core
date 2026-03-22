// Copyright (c) Gesdata. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Gesdata.VF.Contracts.Types;
using Gesdata.VF.Contracts.XML;

namespace Gesdata.VF.Core.XML;

/// <summary>
/// Validador de documentos XML de VeriFactu contra esquemas XSD de AEAT.
/// Patrón consistente con FaceXmlValidator y SepaXmlValidator.
/// </summary>
public static class VFXmlValidator
{
    private static readonly XmlUrlResolver SharedResolver = new EmbeddedAeatResolver();
    private static readonly XmlReaderSettings XsdLoadReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Parse,
        XmlResolver = SharedResolver,
    };

    [ThreadStatic]
    private static VFValidationResult currentResult;
    private static readonly ValidationEventHandler SharedHandler = OnValidationEvent;

    private static void OnValidationEvent(object sender, ValidationEventArgs e)
    {
        var result = currentResult;
        if (result == null)
            return;
        var line = e.Exception?.LineNumber ?? 0;
        var pos = e.Exception?.LinePosition ?? 0;
        var msg = $"{e.Severity}: {e.Message} (Línea {line}, Pos {pos})";
        if (e.Severity == XmlSeverityType.Warning)
            result.Warnings.Add(msg);
        else
            result.Errors.Add(msg);
    }

    /// <summary>
    /// Obtiene la URL oficial de un schema XSD para establecer baseUri y permitir
    /// la resolución de imports relativos (ej: Cabecera.xsd, TiposComunesLR.xsd).
    /// </summary>
    private static string GetOfficialUrl(string logicalPath)
    {
        return logicalPath switch
        {
            VFXsdPathProvider.SuministroInformacion => VFNamespaces.NamespaceSF,
            VFXsdPathProvider.SuministroLR => VFNamespaces.NamespaceSFLR,
            VFXsdPathProvider.RespuestaSuministro => VFNamespaces.NamespaceTikR,
            VFXsdPathProvider.ConsultaLR => VFNamespaces.NamespaceCon,
            VFXsdPathProvider.EventosSIF => VFNamespaces.NamespaceEventosSIF,
            VFXsdPathProvider.XmlDsigCoreSchema => "http://www.w3.org/2000/09/xmldsig#",
            _ => null
        };
    }

    private static void LoadSchemas(XmlSchemaSet schemaSet, string[] xsdPaths, VFValidationResult capture)
    {
        foreach (var path in xsdPaths)
        {
            XmlReader xsdReader = null;
            Stream xsdStream = null;
            var baseUri = GetOfficialUrl(path);
            try
            {
                try
                {
                    xsdStream = VFXsdPathProvider.OpenXsd(path);
                }
                catch (FileNotFoundException)
                {
                    // Fallback: cargar desde URL oficial
                    if (!string.IsNullOrEmpty(baseUri))
                    {
                        xsdReader = XmlReader.Create(baseUri, XsdLoadReaderSettings);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (xsdReader == null)
                {
                    if (xsdStream == null)
                        throw new FileNotFoundException($"No se pudo encontrar el recurso XSD embebido para '{path}'.");

                    xsdReader = baseUri == null
                        ? XmlReader.Create(xsdStream, XsdLoadReaderSettings)
                        : XmlReader.Create(xsdStream, XsdLoadReaderSettings, baseUri);
                }

                using (xsdReader)
                {
                    currentResult = capture;
                    try
                    {
                        var schema = XmlSchema.Read(xsdReader, SharedHandler);
                        if (schema == null)
                            continue;

                        // Evitar duplicados por TNS
                        var tns = schema.TargetNamespace ?? string.Empty;
                        var exists = false;
                        foreach (XmlSchema s in schemaSet.Schemas())
                        {
                            if ((s.TargetNamespace ?? string.Empty) == tns)
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                            schemaSet.Add(schema);
                    }
                    finally
                    {
                        currentResult = null;
                    }
                }
            }
            finally
            {
                xsdStream?.Dispose();
            }
        }
    }

    public static VFValidationResult Validate<T>(T objeto, params string[] xsdPaths)
    {
        var result = new VFValidationResult();
        try
        {
            var schemaSet = new XmlSchemaSet { XmlResolver = SharedResolver };
            LoadSchemas(schemaSet, xsdPaths, result);
            schemaSet.ValidationEventHandler += SharedHandler;

            currentResult = result;
            try
            {
                schemaSet.Compile();
            }
            finally
            {
                currentResult = null;
            }

            if (objeto == null)
            {
                result.FatalException = new ArgumentNullException(nameof(objeto));
                return result;
            }

            var serializer = VFXmlSerializerCache.Get<T>();
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, objeto);
                ms.Position = 0;

                var settings = new XmlReaderSettings
                {
                    Schemas = schemaSet,
                    ValidationType = ValidationType.Schema,
                    XmlResolver = SharedResolver,
                };
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationEventHandler += SharedHandler;

                currentResult = result;
                try
                {
                    using (var reader = XmlReader.Create(ms, settings))
                    {
                        while (reader.Read()) { }
                    }
                }
                finally
                {
                    currentResult = null;
                }
            }
        }
        catch (Exception ex)
        {
            result.FatalException = ex;
        }
        return result;
    }

    public static VFValidationResult ValidateXml(string xml, params string[] xsdPaths)
    {
        var result = new VFValidationResult();
        try
        {
            var schemaSet = new XmlSchemaSet { XmlResolver = SharedResolver };
            LoadSchemas(schemaSet, xsdPaths, result);
            schemaSet.ValidationEventHandler += SharedHandler;

            currentResult = result;
            try
            {
                schemaSet.Compile();
            }
            finally
            {
                currentResult = null;
            }

            var settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema,
                XmlResolver = SharedResolver,
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationEventHandler += SharedHandler;

            currentResult = result;
            try
            {
                using (var sr = new StringReader(xml ?? string.Empty))
                using (var reader = XmlReader.Create(sr, settings))
                {
                    while (reader.Read()) { }
                }
            }
            finally
            {
                currentResult = null;
            }
        }
        catch (Exception ex)
        {
            result.FatalException = ex;
        }
        return result;
    }

    public static VFValidationResult ValidateFile(string xmlFilePath, params string[] xsdPaths)
    {
        var result = new VFValidationResult();
        try
        {
            var schemaSet = new XmlSchemaSet { XmlResolver = SharedResolver };
            LoadSchemas(schemaSet, xsdPaths, result);
            schemaSet.ValidationEventHandler += SharedHandler;

            currentResult = result;
            try
            {
                schemaSet.Compile();
            }
            finally
            {
                currentResult = null;
            }

            var settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema,
                XmlResolver = SharedResolver,
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationEventHandler += SharedHandler;

            currentResult = result;
            try
            {
                using (var fs = File.OpenRead(xmlFilePath))
                using (var reader = XmlReader.Create(fs, settings))
                {
                    while (reader.Read()) { }
                }
            }
            finally
            {
                currentResult = null;
            }
        }
        catch (Exception ex)
        {
            result.FatalException = ex;
        }
        return result;
    }

    public static Result<VFValidationResult> ValidarObjetoResult<T>(T objeto, string xsdPath)
    {
        var res = Validate(objeto, xsdPath);
        return res.IsValid
            ? Result<VFValidationResult>.Ok(res)
            : Result<VFValidationResult>.Fail("XSD no válido", errors: res.Errors);
    }

    public static Result<VFValidationResult> ValidarObjetoResult<T>(T objeto, params string[] xsdPaths)
    {
        var res = Validate(objeto, xsdPaths);
        return res.IsValid
            ? Result<VFValidationResult>.Ok(res)
            : Result<VFValidationResult>.Fail("XSD no válido", errors: res.Errors);
    }

    public static Result<VFValidationResult> ValidarConsultaLRResult(ConsultaFactuSistemaFacturacionType consulta)
        => ValidarObjetoResult(consulta, VFXsdPathProvider.ConsultaLR);

    public static Result<VFValidationResult> ValidarSuministroInformacionResult(RegistroFacturacionAltaType registroAlta)
        => ValidarObjetoResult(registroAlta, VFXsdPathProvider.SuministroInformacion);

    public static Result<VFValidationResult> ValidarSuministroInformacionAnulacionResult(RegistroFacturacionAnulacionType registroAnulacion)
        => ValidarObjetoResult(registroAnulacion, VFXsdPathProvider.SuministroInformacion);

    public static Result<VFValidationResult> ValidarSuministroLRResult(RegFactuSistemaFacturacionType suministro)
        => ValidarObjetoResult(
            suministro,
            VFXsdPathProvider.SuministroLR,
            VFXsdPathProvider.SuministroInformacion,
            VFXsdPathProvider.XmlDsigCoreSchema);

    public static Result<VFValidationResult> ValidarRespuestaSuministroResult(RespuestaRegFactuSistemaFacturacionType respuesta)
        => ValidarObjetoResult(
            respuesta,
            VFXsdPathProvider.RespuestaSuministro,
            VFXsdPathProvider.XmlDsigCoreSchema);

    public static Result<VFValidationResult> ValidarRespuestaValRegistNoVeriFactuResult(RespuestaValContenidoFactuSistemaFacturacionType respuesta)
        => ValidarObjetoResult(respuesta, VFXsdPathProvider.RespuestaValRegistNoVeriFactu);

    public static Result<VFValidationResult> ValidarEventosSIFResult(RegistroEventoType evento)
        => ValidarObjetoResult(
            evento,
            VFXsdPathProvider.EventosSIF,
            VFXsdPathProvider.XmlDsigCoreSchema);

    public static Result<VFValidationResult> ValidarEventoResult(string xml)
    {
        var res = ValidateXml(xml, VFXsdPathProvider.EventosSIF, VFXsdPathProvider.XmlDsigCoreSchema);
        return res.IsValid
            ? Result<VFValidationResult>.Ok(res)
            : Result<VFValidationResult>.Fail("XSD no válido", errors: res.Errors);
    }

    private static string SerializeToString<T>(T objeto)
    {
        if (objeto == null) return string.Empty;
        try
        {
            var serializer = VFXmlSerializerCache.Get<T>();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };
            using var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, settings))
            {
                serializer.Serialize(xw, objeto);
            }
            return sw.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static (VFValidationResult Resultado, string XmlSerializado) ValidateWithXml<T>(T objeto, params string[] xsdPaths)
    {
        var xml = SerializeToString(objeto);
        var res = Validate(objeto, xsdPaths);
        return (res, xml);
    }

    public static (VFValidationResult Resultado, string XmlSerializado) DebugValidarSuministroLR(RegFactuSistemaFacturacionType suministro)
    {
        return ValidateWithXml(
            suministro,
            VFXsdPathProvider.SuministroLR,
            VFXsdPathProvider.XmlDsigCoreSchema);
    }

    /// <summary>
    /// Resolver embebido para schemas XSD de AEAT.
    /// </summary>
    private sealed class EmbeddedAeatResolver : XmlUrlResolver
    {
        private static readonly Assembly ContractsAsm = typeof(VFNamespaces).Assembly;
        private readonly Gesdata.Comun.Logging.LoggingService _logger = new Gesdata.Comun.Logging.LoggingService();

        private static readonly HashSet<string> KnownDsXsdUris = new(StringComparer.OrdinalIgnoreCase)
        {
            "http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/xmldsig-core-schema.xsd",
            "https://www.w3.org/TR/2002/REC-xmldsig-core-20020212/xmldsig-core-schema.xsd",
            "http://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd",
            "https://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd",
        };

        // DTDs de W3C que no necesitan resolverse (usados en declaraciones DOCTYPE de XSD)
        private static readonly HashSet<string> KnownIgnorableDtds = new(StringComparer.OrdinalIgnoreCase)
        {
            "http://www.w3.org/2001/XMLSchema.dtd",
            "https://www.w3.org/2001/XMLSchema.dtd",
            "http://www.w3.org/2001/03/XMLSchema.dtd",
            "https://www.w3.org/2001/03/XMLSchema.dtd",
        };

        private static Stream TryGetEmbeddedStream(string logicalPath)
        {
            var resName = VFXsdPathProvider.ToResourceName(logicalPath);
            return ContractsAsm.GetManifestResourceStream(resName);
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            try
            {
                if (absoluteUri == null)
                    return base.GetEntity(new Uri("about:blank"), role, ofObjectToReturn);

                var abs = absoluteUri.AbsoluteUri;

                // Interceptar referencias a DTD de W3C que no necesitan resolverse
                // Esto evita intentos de descarga web cuando los XSD contienen declaraciones DOCTYPE
                if (abs.EndsWith(".dtd", StringComparison.OrdinalIgnoreCase) &&
                    (abs.Contains("w3.org", StringComparison.OrdinalIgnoreCase) ||
                     abs.Contains("XMLSchema", StringComparison.OrdinalIgnoreCase)))
                {
                    // Normalizar URL (puede estar malformada si se construyó con baseUri incorrecto)
                    var normalizedAbs = abs;
                    if (abs.Contains("/-//W3C//DTD", StringComparison.OrdinalIgnoreCase))
                    {
                        // URL malformada detectada, usar la URL correcta del DTD de XMLSchema
                        normalizedAbs = "http://www.w3.org/2001/XMLSchema.dtd";
                    }

                    if (KnownIgnorableDtds.Contains(normalizedAbs))
                    {
                        // Retornar stream vacío para DTDs conocidos que no son necesarios para validación
                        return new MemoryStream();
                    }
                }

                if (abs.StartsWith("http://www.w3.org/2000/09/xmldsig#", StringComparison.OrdinalIgnoreCase) ||
                    abs.StartsWith("https://www.w3.org/2000/09/xmldsig#", StringComparison.OrdinalIgnoreCase))
                {
                    var stream = TryGetEmbeddedStream(VFXsdPathProvider.XmlDsigCoreSchema);
                    if (stream != null)
                        return stream;
                }

                if (KnownDsXsdUris.Contains(abs))
                {
                    var stream = TryGetEmbeddedStream(VFXsdPathProvider.XmlDsigCoreSchema);
                    if (stream != null)
                        return stream;
                }

                if (absoluteUri.Host.Contains("agenciatributaria.gob.es", StringComparison.OrdinalIgnoreCase))
                {
                    var file = Path.GetFileName(absoluteUri.AbsolutePath);
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        var stream = TryGetEmbeddedStream($"Esquemas/{file}");
                        if (stream != null)
                            return stream;
                    }
                }

                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError("VFXmlValidator", $"[GetEntity] ❌ Error: {ex.Message}");
                return null;
            }
        }
    }
}
