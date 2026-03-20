using Gesdata.Comun.Formatos;
using Gesdata.VF.Contracts.EnumTypes;
using Gesdata.VF.Contracts.Types;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Gesdata.VF.Core.XML
{
    /// <summary>
    /// UtilComun para el cálculo de la huella (hash) exigida por VERI*FACTU. Implementa SHA-256 sobre la concatenación
    /// NIF|NumSerie|FechaExpedición|HuellaAnterior|FechaHoraGen (o la variante específica según el tipo de registro).
    /// </summary>
    public static class VFHuella
    {
        /// <summary>
        /// Calcula la huella (SHA-256 en HEX) de un registro de facturación a partir del identificador de factura, la
        /// huella anterior de la cadena (si existe) y la fecha/hora de generación del registro (con huso).
        /// </summary>
        /// <param name="idFactura">Identificación de la factura (NIF emisor, número de serie y fecha).</param>
        /// <param name="huellaAnterior">Huella del registro anterior en la cadena, si existe.</param>
        /// <param name="fechaHoraHusoGen">Fecha y hora (con huso) de generación del registro.</param>
        /// <returns>Cadena hexadecimal de 64 caracteres (resultado de aplicar SHA-256).</returns>
        public static string Compute(IDFacturaExpedidaType idFactura, string huellaAnterior, DateTime fechaHoraHusoGen)
        {
            if (idFactura is null)
                return string.Empty;
            var nif = (idFactura.IDEmisorFactura ?? string.Empty).Trim();
            var num = (idFactura.NumSerieFactura ?? string.Empty).Trim();
            var fec = (idFactura.FechaExpedicionFactura ?? string.Empty).Trim();
            var prev = (huellaAnterior ?? string.Empty).Trim();
            var fechaGen = SpanishFormat.DateTimeIso8601WithK(fechaHoraHusoGen);
            var payload = string.Join("|", new[] { nif, num, fec, prev, fechaGen });
            return Sha256Hex(payload);
        }

        /// <summary>
        /// Art.13.a: Huella para registro de facturación de alta.
        /// Formato AEAT: IDEmisorFactura=X&NumSerieFactura=Y&FechaExpedicionFactura=Z&TipoFactura=T&CuotaTotal=C&ImporteTotal=I&Huella=H&FechaHoraHusoGenRegistro=F
        /// </summary>
        public static string ComputeAlta(RegistroFacturacionAltaType alta, string huellaAnterior)
        {
            // ✅ VALIDACIÓN: Prevenir NullReferenceException
            if (alta is null)
                return string.Empty;

            if (alta.IDFactura is null)
                throw new ArgumentNullException(nameof(alta.IDFactura), "IDFactura es obligatorio para ComputeAlta");

            var id = alta.IDFactura;
            var nif = (id.IDEmisorFactura ?? string.Empty).Trim();
            var num = (id.NumSerieFactura ?? string.Empty).Trim();
            var fec = (id.FechaExpedicionFactura ?? string.Empty).Trim();
            // ✅ FIX: Usar código XML del enum (F1, F2, R1...) en lugar del nombre
            var tipo = GetXmlEnumValue(alta.TipoFactura);
            var cuota = (alta.CuotaTotal ?? string.Empty).Trim();
            var importe = (alta.ImporteTotal ?? string.Empty).Trim();
            var prev = (huellaAnterior ?? alta.Encadenamiento?.RegistroAnterior?.Huella ?? string.Empty).Trim();
            // FechaHoraHusoGenRegistro ahora es string (formato ISO 8601)
            var fechaGen = (alta.FechaHoraHusoGenRegistro ?? string.Empty).Trim();

            // ✅ FIX CRÍTICO: Usar formato URL-encoded con & como separador (según respuesta AEAT)
            // AEAT espera: IDEmisorFactura=B81435463&NumSerieFactura=TEST-000000/01&FechaExpedicionFactura=18-11-2025&TipoFactura=F1&CuotaTotal=0.00&ImporteTotal=817.38&Huella=&FechaHoraHusoGenRegistro=2025-11-18T16:01:15+01:00
            var payload = $"IDEmisorFactura={nif}&NumSerieFactura={num}&FechaExpedicionFactura={fec}&TipoFactura={tipo}&CuotaTotal={cuota}&ImporteTotal={importe}&Huella={prev}&FechaHoraHusoGenRegistro={fechaGen}";

            return Sha256Hex(payload);
        }

        /// <summary>
        /// Art.13.b: Huella para registro de facturación de anulación.
        /// Formato AEAT: IDEmisorFacturaAnulada=X&NumSerieFacturaAnulada=Y&FechaExpedicionFacturaAnulada=Z&Huella=H&FechaHoraHusoGenRegistro=F
        /// IMPORTANTE: La huella H es la del ÚLTIMO REGISTRO DE LA CADENA (NO la de la factura que se anula).
        /// </summary>
        /// <remarks>
        /// <para><b>Cálculo correcto según normativa AEAT:</b></para>
        /// <list type="bullet">
        ///   <item>IDEmisorFacturaAnulada = NIF de la factura que SE ANULA</item>
        ///   <item>NumSerieFacturaAnulada = Número/serie de la factura que SE ANULA</item>
        ///   <item>FechaExpedicionFacturaAnulada = Fecha de la factura que SE ANULA</item>
        ///   <item>Huella = Huella del ÚLTIMO REGISTRO de la cadena (encadenamiento de continuidad)</item>
        ///   <item>FechaHoraHusoGenRegistro = Timestamp de generación del registro de anulación</item>
        /// </list>
        /// 
        /// <para><b>Ejemplo:</b></para>
        /// <code>
        /// // Último registro en cadena: FR-000112/13 (huella: ABCD...)
        /// // Factura a anular: FR-000112/12 (emitida hace 2 días, huella original: 3E1A...)
        /// 
        /// payload = "IDEmisorFacturaAnulada=B81435463" +
        ///           "&NumSerieFacturaAnulada=FR-000112/12" +  // ← Factura que anulamos
        ///           "&FechaExpedicionFacturaAnulada=13-11-2025" +
        ///           "&Huella=ABCD..." +  // ← Huella del ÚLTIMO registro (FR-000112/13), NO de FR-000112/12
        ///           "&FechaHoraHusoGenRegistro=2025-11-19T18:40:41+01:00";
        /// </code>
        /// </remarks>
        /// <param name="anul">DTO de anulación con IDFactura (factura anulada) y Encadenamiento (último registro cadena)</param>
        /// <param name="huellaAnterior">Huella del ÚLTIMO REGISTRO de la cadena (encPrev.Huella). 
        /// ADVERTENCIA: NO confundir con la huella de la factura que se anula.</param>
        public static string ComputeAnulacion(RegistroFacturacionAnulacionType anul, string huellaAnterior)
        {
            if (anul is null)
                return string.Empty;
            var id = anul.IDFactura ?? new IDFacturaExpedidaBajaType();
            var nif = (id.IDEmisorFacturaAnulada ?? string.Empty).Trim();
            var num = (id.NumSerieFacturaAnulada ?? string.Empty).Trim();
            var fec = (id.FechaExpedicionFacturaAnulada ?? string.Empty).Trim();

            // ✅ CORRECCIÓN CRÍTICA: Usar huella del ÚLTIMO REGISTRO de la cadena
            // NO usar la huella de la factura que se anula (esa va en IDFactura)
            // El parámetro huellaAnterior DEBE ser encPrev.Huella (último registro)
            var prev = (huellaAnterior ?? anul.Encadenamiento?.RegistroAnterior?.Huella ?? string.Empty).Trim();

            // FechaHoraHusoGenRegistro ahora es string (formato ISO 8601)
            var fechaGen = (anul.FechaHoraHusoGenRegistro ?? string.Empty).Trim();

            // ✅ FIX CRÍTICO: Usar nombres CON SUFIJO "Anulada" (según respuesta AEAT error 2000)
            // AEAT espera: IDEmisorFacturaAnulada=B81435463&NumSerieFacturaAnulada=FR-000112/12&FechaExpedicionFacturaAnulada=13-11-2025&Huella=ABCD...&FechaHoraHusoGenRegistro=2025-11-19T18:40:41+01:00
            // Donde Huella=ABCD... es la huella del ÚLTIMO registro de la cadena (NO de FR-000112/12)
            var payload = $"IDEmisorFacturaAnulada={nif}&NumSerieFacturaAnulada={num}&FechaExpedicionFacturaAnulada={fec}&Huella={prev}&FechaHoraHusoGenRegistro={fechaGen}";

            return Sha256Hex(payload);
        }

        /// <summary>
        /// Art.13.c: Huella para registro de evento a partir del tipo EventoType.
        /// IdProductor + IdSistema + VersionSistema + NumeroInstalacion + NIFObligado + TipoEvento + HuellaAnterior + FechaHoraGen.
        /// </summary>
        public static string ComputeEvento(EventoType evento, string huellaAnterior)
        {
            if (evento is null)
                return string.Empty;
            var si = evento.SistemaInformatico ?? new SistemaInformaticoType();
            var productor = (si.NombreRazon ?? string.Empty).Trim();
            var idSis = (si.IdSistemaInformatico ?? string.Empty).Trim();
            var ver = (si.Version ?? string.Empty).Trim();
            var numInst = (si.NumeroInstalacion ?? string.Empty).Trim();
            var nifObl = (evento.ObligadoEmision?.NIF ?? string.Empty).Trim();
            var tipo = evento.TipoEvento.ToString();
            // Ajuste: EncadenamientoEventoType ya no expone RegistroAnterior.Huella sino EventoAnterior.HuellaEvento
            var prev = (huellaAnterior ?? evento.Encadenamiento?.EventoAnterior?.HuellaEvento ?? string.Empty).Trim();
            var fechaGen = SpanishFormat.DateTimeIso8601WithK(evento.FechaHoraHusoGenEvento);

            // ✅ CORRECCIÓN CRÍTICA: Concatenación directa SIN separadores
            // Antes: var payload = string.Join("|", new[] { productor, idSis, ver, numInst, nifObl, tipo, prev, fechaGen });
            var payload = $"{productor}{idSis}{ver}{numInst}{nifObl}{tipo}{prev}{fechaGen}";

            return Sha256Hex(payload);
        }

        /// <summary>
        /// Art.13.c: Huella para registro de evento usando campos primitivos (sin construir EventoType).
        /// IdProductor + IdSistema + VersionSistema + NumeroInstalacion + NIFObligado + TipoEvento + HuellaAnterior + FechaHoraGen.
        /// </summary>
        public static string ComputeEvento(string productor, string idSistema, string version, string numeroInstalacion, string nifObligado, TipoEventoType tipo, string huellaAnterior, DateTime fechaHoraHusoGen)
        {
            var prod = (productor ?? string.Empty).Trim();
            var idSis = (idSistema ?? string.Empty).Trim();
            var ver = (version ?? string.Empty).Trim();
            var numInst = (numeroInstalacion ?? string.Empty).Trim();
            var nifObl = (nifObligado ?? string.Empty).Trim();
            var tipoStr = tipo.ToString();
            var prev = (huellaAnterior ?? string.Empty).Trim();
            var fechaGen = SpanishFormat.DateTimeIso8601WithK(fechaHoraHusoGen);

            // ✅ CORRECCIÓN CRÍTICA: Concatenación directa SIN separadores
            // Antes: var payload = string.Join("|", new[] { prod, idSis, ver, numInst, nifObl, tipoStr, prev, fechaGen });
            var payload = $"{prod}{idSis}{ver}{numInst}{nifObl}{tipoStr}{prev}{fechaGen}";

            return Sha256Hex(payload);
        }

        /// <summary>
        /// Obtiene el valor XML de un enum decorado con [XmlEnum].
        /// </summary>
        private static string GetXmlEnumValue<T>(T enumValue) where T : Enum
        {
            var type = enumValue.GetType();
            var memberInfo = type.GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(XmlEnumAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((XmlEnumAttribute)attrs[0]).Name;
                }
            }
            return enumValue.ToString();
        }

        /// <summary>
        /// Calcula SHA-256 de una cadena y devuelve el resultado en hexadecimal (mayúsculas).
        /// </summary>
        /// <param name="input">Entrada a digerir.</param>
        /// <returns>Cadena hexadecimal del hash (o cadena vacía si la entrada es nula).</returns>
        public static string Sha256Hex(string input)
        {
            if (input is null)
                return string.Empty;
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.AppendFormat("{0:X2}", b);
            return sb.ToString();
        }
    }
}
