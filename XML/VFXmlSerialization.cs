// #nullable enable
using Gesdata.Comun.Xml.Core;
using Gesdata.VF.Contracts.XML;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PooledStringWriter = Gesdata.Comun.Xml.Performance.PooledStringWriter;
using StringBuilderPool = Gesdata.Comun.Xml.Performance.StringBuilderPool;
// ✅ Aliases para usar infraestructura de Gesdata.Comun sin cambiar código existente
using XmlSettingsCache = Gesdata.Comun.Xml.Core.XmlSettingsCache;

namespace Gesdata.VF.Core.XML
{
    /// <summary>
    /// UtilComun para crear espacios de nombres XML y (de)serializar objetos
    /// conforme a los XSD/servicios de la AEAT utilizando <see cref="VFNamespaces"/>.
    /// ✅ REFACTORIZADO: Utiliza infraestructura de Gesdata.Comun.Xml para optimización.
    /// </summary>
    public static class VFXmlSerialization
    {
        /// <summary>
        /// Encoding UTF-8 sin BOM con WebName en MAYÚSCULAS (requerido por AEAT).
        /// </summary>
        private static readonly Encoding DefaultEncoding = new Utf8EncodingUpperCase();

        /// <summary>
        /// Crea los espacios de nombres SOLO para RegFactu (sum + sum1).
        /// Para que WCF use estos prefijos correctamente en el SOAP Envelope.
        /// </summary>
        public static XmlSerializerNamespaces CreateRegFactuNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("sum", VFNamespaces.NamespaceSFLR);    // SuministroLR.xsd
            ns.Add("sum1", VFNamespaces.NamespaceSF);     // SuministroInformacion.xsd
            return ns;
        }

        /// <summary>
        /// Crea los espacios de nombres para ConsultaLR (con + sum1).
        /// Las consultas NO se firman, por lo que NO se incluye namespace de xmldsig.
        /// </summary>
        public static XmlSerializerNamespaces CreateConsultaNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("con", VFNamespaces.NamespaceCon);     // ConsultaLR.xsd
            ns.Add("sum1", VFNamespaces.NamespaceSF);     // SuministroInformacion.xsd (tipos compartidos)
            return ns;
        }

        /// <summary>
        /// Crea un XmlSerializerNamespaces VACÍO para que no se incluyan namespaces en el elemento raíz.
        /// WCF agregará los namespaces necesarios en el SOAP Envelope.
        /// </summary>
        public static XmlSerializerNamespaces CreateEmptyNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty); // Fuerza a no incluir xmlns en el elemento raíz
            return ns;
        }

        /// <summary>
        /// Crea los espacios de nombres por defecto usando <see cref="VFNamespaces.Items"/>.
        /// ✅ Los namespaces son SIEMPRE de producción (www2.agenciatributaria.gob.es)
        /// independientemente del endpoint usado.
        /// Incluye prefijos: soapenv, sum, sum1, con, tik, tiklr, evt.
        /// </summary>
        public static XmlSerializerNamespaces CreateDefaultNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            foreach (var kvp in VFNamespaces.Items)
                ns.Add(kvp.Key, kvp.Value);
            return ns;
        }

        /// <summary>
        /// Crea los espacios de nombres según el endpoint proporcionado.
        /// ✅ IMPORTANTE: Los namespaces son SIEMPRE de producción (www2.agenciatributaria.gob.es)
        /// independientemente de si el endpoint es de preproducción o producción.
        /// </summary>
        /// <param name="endpoint">URI del endpoint de AEAT (puede ser pre o prod).</param>
        public static XmlSerializerNamespaces CreateNamespacesPorEndpoint(Uri endpoint)
        {
            // ✅ Usar siempre Namespaces.Items (producción)
            // Los namespaces NO cambian según el endpoint
            return CreateDefaultNamespaces();
        }

        /// <summary>
        /// Crea los espacios de nombres según el endpoint proporcionado.
        /// ✅ IMPORTANTE: Los namespaces son SIEMPRE de producción (www2.agenciatributaria.gob.es)
        /// independientemente de si el endpoint es de preproducción o producción.
        /// </summary>
        /// <param name="endpoint">URL del endpoint de AEAT (puede ser pre o prod).</param>
        public static XmlSerializerNamespaces CreateNamespacesPorEndpoint(string endpoint)
        {
            // ✅ Usar siempre Namespaces.Items (producción)
            // Los namespaces NO cambian según el endpoint
            return CreateDefaultNamespaces();
        }

        /// <summary>
        /// Crea los espacios de nombres para el servicio de validación de NIF.
        /// Incluye prefijos: soapenv, VNifV2Ent, VNifV2Sal.
        /// </summary>
        public static XmlSerializerNamespaces CreateNifNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            foreach (var kvp in VFNamespaces.NifItems)
                ns.Add(kvp.Key, kvp.Value);
            return ns;
        }

        /// <summary>
        /// Serializa un objeto a XML aplicando los espacios de nombres indicados.
        /// Por defecto usa <see cref="CreateDefaultNamespaces"/>.
        /// ✅ REFACTORIZADO: Delega a XmlSerializerHelper con encoding AEAT.
        /// </summary>
        public static string Serialize<T>(
            T value,
            XmlSerializerNamespaces namespaces = null,
            bool omitXmlDeclaration = false,
            Encoding encoding = null,
            bool indent = false)
        {
            if (value == null)
                return string.Empty;

            // ✅ Aplicar defaults específicos AEAT
            namespaces ??= CreateDefaultNamespaces();
            encoding ??= DefaultEncoding;

            // ✅ Usar XmlSerializerHelper de Gesdata.Comun pero con cache VF.Core
            var serializer = VFXmlSerializerCache.Get<T>();
            var settings = XmlSettingsCache.GetWriterSettings(omitXmlDeclaration, indent, encoding);

            var sb = StringBuilderPool.Rent(4096);
            try
            {
                using var writer = new PooledStringWriter(sb, encoding);
                using var xmlWriter = XmlWriter.Create(writer, settings);

                if (namespaces != null)
                    serializer.Serialize(xmlWriter, value, namespaces);
                else
                    serializer.Serialize(xmlWriter, value);

                xmlWriter.Flush();
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Variante que acepta CancellationToken (para cancelaciones cooperativas).
        /// ✅ REFACTORIZADO: Delega a versión principal.
        /// </summary>
        public static string Serialize<T>(
            T value,
            XmlSerializerNamespaces namespaces,
            bool omitXmlDeclaration,
            Encoding encoding,
            bool indent,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Serialize(value, namespaces, omitXmlDeclaration, encoding, indent);
        }

        /// <summary>
        /// Crea un XmlRootAttribute para forzar nombre y namespace del elemento raíz.
        /// </summary>
        public static XmlRootAttribute CreateRootOverride(string elementName, string @namespace)
            => new(elementName) { Namespace = @namespace, IsNullable = false };

        /// <summary>
        /// Deserializa un XML a T. Si se pasa rootOverride, se usa como raíz esperada.
        /// ✅ REFACTORIZADO: Usa cache VF.Core con infraestructura Gesdata.Comun.
        /// </summary>
        public static T Deserialize<T>(string xml, XmlRootAttribute rootOverride = null)
        {
            XmlSerializer serializer;
            if (rootOverride is null)
            {
                // Usar serializer precompilado del cache VF.Core
                serializer = VFXmlSerializerCache.Get<T>();
            }
            else
            {
                // Si hay override de raíz, usar cache específico
                serializer = VFXmlSerializerCache.Get(typeof(T), rootOverride);
            }

            using var sr = new StringReader(xml);
            using var xr = XmlReader.Create(sr, XmlSettingsCache.GetReaderSettings());
            return (T)serializer.Deserialize(xr)!;
        }

        /// <summary>
        /// Deserializa directamente desde un XmlReader.
        /// ✅ NUEVO: Preserva mejor los namespaces hijos que deserializar desde string.
        /// </summary>
        public static T Deserialize<T>(XmlReader reader, XmlRootAttribute rootOverride = null)
        {
            XmlSerializer serializer = rootOverride is null ? VFXmlSerializerCache.Get<T>() : VFXmlSerializerCache.Get(typeof(T), rootOverride);
            return (T)serializer.Deserialize(reader)!;
        }

        /// <summary>
        /// Deserializa desde fichero a T. Permite especificar raíz esperada con namespace.
        /// </summary>
        public static T DeserializeFromFile<T>(string filePath, XmlRootAttribute rootOverride = null)
            => Deserialize<T>(File.ReadAllText(filePath, new UTF8Encoding(false)), rootOverride);

        /// <summary>
        /// Deserializa desde fichero a T usando lectura asíncrona y CancellationToken.
        /// </summary>
        public static async Task<T> DeserializeFromFileAsync<T>(
            string filePath,
            XmlRootAttribute rootOverride = null,
            CancellationToken ct = default)
        {
            var xml = await File.ReadAllTextAsync(filePath, new UTF8Encoding(false), ct).ConfigureAwait(false);
            return Deserialize<T>(xml, rootOverride);
        }
    }
}