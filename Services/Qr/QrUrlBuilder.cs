using Gesdata.Comun.Formatos;
using Gesdata.VF.Contracts.Types;
using Gesdata.VF.Contracts.XML;
using System.Net;
using System.Text;

namespace Gesdata.VF.Core.Services.Qr
{
    /// <summary>
    /// Generador de URL de cotejo AEAT para el código QR de factura (Arts.20-21) según especificación técnica.
    /// Enfoque exclusivo VERI*FACTU.
    /// ✅ SIMPLIFICADO: No valida datos (ya validados en entidades/servicios antes de llegar aquí).
    /// </summary>
    public static class QrUrlBuilder
    {
        /// <summary>
        /// Construye la URL VeriFactu para código QR.
        /// ⚠️ PRECONDICIÓN: Los datos deben estar validados antes de llamar a este método.
        /// </summary>
        /// <param name="nif">NIF del emisor (ya validado)</param>
        /// <param name="numserie">Número de serie de la factura (ya validado)</param>
        /// <param name="fechaExpedicion">Fecha de expedición (ya validada)</param>
        /// <param name="importeTotal">Importe total (ya validado)</param>
        /// <param name="isProduction">Si es null, usa VerifactuSettings actual. Si se especifica, lo sobrescribe.</param>
        /// <returns>URL completa del código QR VeriFactu</returns>
        public static string BuildVerifactuQrUrl(
            string nif,
            string numserie,
            DateTime fechaExpedicion,
            decimal importeTotal,
            bool isProduction)
        {
            // ✅ Solo defensive programming básico (no validaciones complejas)
            ArgumentException.ThrowIfNullOrWhiteSpace(nif);
            ArgumentException.ThrowIfNullOrWhiteSpace(numserie);

            // Formatear datos según especificación AEAT
            var fecha = SpanishFormat.DateDmy(fechaExpedicion);
            var importe = SpanishFormat.Amount(importeTotal, 2);

            // Construir URL
            var baseUrl = GetBaseUrl(isProduction);

            var sb = new StringBuilder(baseUrl);
            sb.Append(baseUrl.Contains('?') ? '&' : '?');
            sb.Append("nif=").Append(WebUtility.UrlEncode(nif));
            sb.Append("&numserie=").Append(WebUtility.UrlEncode(numserie));
            sb.Append("&fecha=").Append(WebUtility.UrlEncode(fecha));
            sb.Append("&importe=").Append(WebUtility.UrlEncode(importe));

            return sb.ToString();
        }

        /// <summary>
        /// Construye la URL VeriFactu desde un IDFacturaType.
        /// </summary>
        public static string BuildVerifactuQrUrl(
            IDFacturaType idFactura,
            DateTime fechaExpedicion,
            decimal importeTotal,
            bool isProduction)// = null)
        {
            ArgumentNullException.ThrowIfNull(idFactura);

            return BuildVerifactuQrUrl(
                idFactura.IDEmisorFactura,
                idFactura.NumSerieFactura,
                fechaExpedicion,
                importeTotal,
                isProduction);
        }

        /// <summary>
        /// Devuelve la URL VERI*FACTU con el parámetro adicional de formato JSON (para consumo programático).
        /// </summary>
        public static string BuildVerifactuQrJsonUrl(
            string nif,
            string numserie,
            DateTime fechaExpedicion,
            decimal importeTotal,
            bool isProduction)
        {
            var url = BuildVerifactuQrUrl(nif, numserie, fechaExpedicion, importeTotal, isProduction);
            return url + "&formato=json";
        }

        /// <summary>
        /// Devuelve la URL base según entorno PRE/PROD usando Namespaces.WsEndpoints.
        /// </summary>
        private static string GetBaseUrl(bool isProduction)
        {
            //var isProd = isProduction ?? TryGetProductionFromSettings();

            return isProduction
                ? VFNamespaces.WsEndpoints.ValidarQrProd
                : VFNamespaces.WsEndpoints.ValidarQrPre;
        }

        /// <summary>
        /// Intenta leer IsProduction desde VerifactuSettings.
        /// Si no está disponible, devuelve false (PRE por seguridad).
        /// </summary>
        //private static bool TryGetProductionFromSettings()
        //{
        //    try
        //    {
        //        // ✅ SOLUCIÓN: Acceder a VeriFactuConfig desde App
        //        // VeriFactuConfig se carga en el constructor estático de App desde verifactu-settings.json
        //        var config = Gesdata.App.VeriFactuConfig;
        //        return config?.IsProduction ?? false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}
