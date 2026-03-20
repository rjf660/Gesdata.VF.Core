namespace Gesdata.VF.Core.XML
{
    public static class VFXsdPathProvider
    {
        public const string ConsultaLR = "Esquemas/ConsultaLR.xsd";
        public const string SuministroInformacion = "Esquemas/SuministroInformacion.xsd";
        public const string SuministroLR = "Esquemas/SuministroLR.xsd";
        public const string RespuestaSuministro = "Esquemas/RespuestaSuministro.xsd";
        public const string RespuestaConsultaLR = "Esquemas/RespuestaConsultaLR.xsd";
        public const string RespuestaValRegistNoVeriFactu = "Esquemas/RespuestaValRegistNoVeriFactu.xsd"; // moved up
        public const string EventosSIF = "Esquemas/EventosSIF.xsd"; // moved up
        public const string XmlDsigCoreSchema = "Esquemas/xmldsig-core-schema.xsd";

        // Devuelve un flujo al recurso XSD embebido dentro del ensamblado Gesdata.VF.Contracts
        public static Stream OpenXsd(string logicalPath)
        {
            // Los XSD están embebidos en Gesdata.VF.Contracts con RootNamespace = Gesdata.VF.Contracts
            var asm = typeof(Gesdata.VF.Contracts.XML.VFNamespaces).Assembly; // Ensamblado de Contracts
            var resourceName = ToResourceName(logicalPath);
            var stream = asm.GetManifestResourceStream(resourceName);

            return stream is null
                ? throw new FileNotFoundException($"No se encontró el recurso XSD embebido '{resourceName}' para '{logicalPath}'.")
                : stream;
        }

        // Traduce la ruta lógica (con / o \) a nombre de recurso manifest
        public static string ToResourceName(string logicalPath)
        {
            var path = logicalPath.Replace('\\', '.').Replace('/', '.');
            // Los recursos se vinculan como Esquemas/... bajo el RootNamespace del contrato
            return $"Gesdata.VF.Contracts.{path}";
        }
    }
}