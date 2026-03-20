using Gesdata.VF.Contracts.XML;

namespace Gesdata.VF.Core.XML
{
    /// <summary>
    /// Endpoints oficiales TIKE-CONT (VERI*FACTU) expuestos como <see cref="Uri"/> listos para usar.
    /// </summary>
    /// <remarks>
    /// Se construyen a partir de <see cref="VFNamespaces.WsEndpoints"/> para mantener una única fuente de URLs.
    /// Usa <see cref="VerifactuPre"/> para integración/pruebas y <see cref="VerifactuProd"/> en producción.
    /// Los sufijos "Sello" suelen referirse a endpoints para sellado o variaciones del entorno.
    /// </remarks>
    public static class VFEndpoints
    {
        /// <summary>Endpoint de producción para el servicio Verifactu.</summary>
        public static readonly Uri VerifactuProd = new(VFNamespaces.WsEndpoints.VerifactuProd);

        /// <summary>Endpoint de producción (variante Sello) para el servicio Verifactu.</summary>
        public static readonly Uri VerifactuProdSello = new(VFNamespaces.WsEndpoints.VerifactuProdSello);

        /// <summary>Endpoint de preproducción para el servicio Verifactu.</summary>
        public static readonly Uri VerifactuPre = new(VFNamespaces.WsEndpoints.VerifactuPre);

        /// <summary>Endpoint de preproducción (variante Sello) para el servicio Verifactu.</summary>
        public static readonly Uri VerifactuPreSello = new(VFNamespaces.WsEndpoints.VerifactuPreSello);

        /// <summary>Endpoint de producción para el servicio Requerimiento.</summary>
        public static readonly Uri RequerimientoProd = new(VFNamespaces.WsEndpoints.RequerimientoProd);

        /// <summary>Endpoint de producción (variante Sello) para el servicio Requerimiento.</summary>
        public static readonly Uri RequerimientoProdSello = new(VFNamespaces.WsEndpoints.RequerimientoProdSello);

        /// <summary>Endpoint de preproducción para el servicio Requerimiento.</summary>
        public static readonly Uri RequerimientoPre = new(VFNamespaces.WsEndpoints.RequerimientoPre);

        /// <summary>Endpoint de preproducción (variante Sello) para el servicio Requerimiento.</summary>
        public static readonly Uri RequerimientoPreSello = new(VFNamespaces.WsEndpoints.RequerimientoPreSello);
    }
}
