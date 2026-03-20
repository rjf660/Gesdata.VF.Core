namespace Gesdata.VF.Core.Configuration
{
    /// <summary>
    /// Modo de operación del sistema de facturación según normativa AEAT.
    /// Controla qué artículos de la Orden HAC/1177/2024 son aplicables.
    /// </summary>
    public enum ModoSistemaFacturacion
    {
        /// <summary>
        /// Sistema de emisión de facturas verificables (VERI*FACTU).
        /// ✅ Art. 3 Orden HAC/1177/2024: Exento de Arts. 6.b-f, 7.f/h/i/j, 8 y 9.
        /// ✅ NO genera registros de evento (Art. 9).
        /// ✅ Envío online a AEAT.
        /// ✅ Modo por defecto recomendado.
        /// </summary>
        VeriFactu = 1,

        /// <summary>
        /// Sistema informático NO VeriFactu (tradicional).
        /// ⚠️ Debe cumplir TODOS los artículos (6, 7, 8 y 9).
        /// ⚠️ Genera registros de evento obligatorios (Art. 9).
        /// ⚠️ Conservación local + exportación manual.
        /// ⚠️ Usar solo si cliente lo requiere explícitamente.
        /// </summary>
        NoVeriFactu = 2
    }
    public enum AeatEnvironment
    {
        Produccion = 1,
        Preproduccion = 2
    }

    /// <summary>
    /// Configuración de VeriFactu.
    /// ✅ CONSOLIDADO: Toda la configuración de VeriFactu en un solo lugar.
    /// ✅ MODO VERIFACTU: Exento de Art. 9 (registro de eventos).
    /// </summary>
    public sealed class VerifactuSettings
    {
        // ✅ Configuración de entorno
        public bool IsProduction { get; set; } = false;

        // ✅ NUEVO: Configuración de depuración SOAP
        /// <summary>
        /// Indica si se guardan los mensajes SOAP en archivos para depuración.
        /// Por defecto: true en Development, false en Production.
        /// Si está activado, tampoco se guardan los registros de 'PermissionsPendientes' en BD.
        /// </summary>
        public bool SaveDebugSoap { get; set; }

        /// <summary>
        /// Carpeta donde se guardan los archivos SOAP de depuración.
        /// Por defecto: Desktop del usuario.
        /// Puedes cambiarlo a una ruta específica (ej: "C:\\Logs\\VeriFactu\\SOAP").
        /// </summary>
        public string DebugSoapFolder { get; set; } = null;

        /// <summary>
        /// ⚠️ SOLO PARA TESTING: Simula error de huella en respuestas de AEAT.
        /// NUNCA habilitar en producción.
        /// </summary>
        public bool SimularErrorHuella { get; set; } = false;

        /// <summary>
        /// Configuración de comunicación con AEAT.
        /// </summary>
        public AeatSettings Aeat { get; set; } = new();

        /// <summary>
        /// Configuración de códigos QR VeriFactu.
        /// </summary>
        public QrSettings Qr { get; set; } = new();

        /// <summary>
        /// Configuración de política de firma XAdES EPES.
        /// </summary>
        public SignaturePolicySettings SignaturePolicy { get; set; } = new();

        /// <summary>
        /// Configuración de reintentos diferidos (VeriFactuRetryService).
        /// Implementa Art. 16.4 RD 1007/2023.
        /// </summary>
        public RetrySettings Retry { get; set; } = new();

        /// <summary>
        /// Versión del esquema XML VeriFactu.
        /// ✅ MIGRADO desde RegistroType.IDVersion = "1.0"
        /// Default: "1.0"
        /// </summary>
        public string XmlSchemaVersion { get; set; } = "1.0";
    }

    /// <summary>
    /// Configuración de comunicación con AEAT.
    /// </summary>
    public sealed class AeatSettings
    {
        public string Endpoint { get; set; } = string.Empty;

        // ✅ Timeouts de binding
        public int SendTimeoutSeconds { get; set; } = 120;
        public int ReceiveTimeoutSeconds { get; set; } = 120;
        public int OpenTimeoutSeconds { get; set; } = 30;
        public int CloseTimeoutSeconds { get; set; } = 30;

        // ✅ Timeout de operación (WCF)
        public int OperationTimeoutSeconds { get; set; } = 120;

        // ✅ Validación de certificados del servidor
        public bool ValidateServerCertificate { get; set; } = true;
        public string RevocationMode { get; set; } = "Online"; // "Online" | "Offline" | "NoCheck"
        public string TrustedStoreLocation { get; set; } = "LocalMachine"; // "LocalMachine" | "CurrentUser"

        // ✅ Reintentos INMEDIATOS (Polly inline)
        public int RetryCount { get; set; } = 2;
        public int RetryBaseDelayMilliseconds { get; set; } = 1000;
        public double RetryBackoffFactor { get; set; } = 2.0;
    }

    /// <summary>
    /// Configuración de reintentos DIFERIDOS (background service).
    /// Implementa Art. 16.4 RD 1007/2023: reintentos tras fallos transitorios.
    /// </summary>
    public sealed class RetrySettings
    {
        /// <summary>
        /// Número máximo de intentos diferidos (después del intento inicial).
        /// Default: 4 (total 5 intentos: 1 inicial + 4 reintentos).
        /// </summary>
        public int MaxAttempts { get; set; } = 4;

        /// <summary>
        /// Intervalo de polling del servicio de reintentos (minutos).
        /// Default: 2 minutos.
        /// </summary>
        public int PollingIntervalMinutes { get; set; } = 2;

        /// <summary>
        /// Delays entre reintentos (minutos).
        /// ✅ MIGRADO desde VeriFactuRetryService.ComputeNextDelay()
        /// </summary>
        public RetryDelaysSettings Delays { get; set; } = new();
    }

    /// <summary>
    /// Delays progresivos para reintentos diferidos.
    /// </summary>
    public sealed class RetryDelaysSettings
    {
        /// <summary>
        /// Delay del 1er reintento (minutos). Default: 5 min.
        /// </summary>
        public int FirstRetryMinutes { get; set; } = 5;

        /// <summary>
        /// Delay del 2º reintento (minutos). Default: 15 min.
        /// </summary>
        public int SecondRetryMinutes { get; set; } = 15;

        /// <summary>
        /// Delay del 3er reintento (minutos). Default: 30 min.
        /// </summary>
        public int ThirdRetryMinutes { get; set; } = 30;

        /// <summary>
        /// Delay del 4º reintento (minutos). Default: 60 min.
        /// </summary>
        public int FourthRetryMinutes { get; set; } = 60;

        /// <summary>
        /// Delay máximo para reintentos posteriores (horas). Default: 6 horas.
        /// </summary>
        public int MaxDelayHours { get; set; } = 6;

        /// <summary>
        /// Factor de backoff exponencial para reintentos >= 5.
        /// Default: 2.0 (duplica el delay cada vez).
        /// </summary>
        public double BackoffFactor { get; set; } = 2.0;
    }

    /// <summary>
    /// Configuración de códigos QR VeriFactu.
    /// </summary>
    public sealed class QrSettings
    {
        /// <summary>
        /// Entorno QR: "Pre" | "Prod"
        /// </summary>
        public AeatEnvironment Environment { get; set; }
    }

    /// <summary>
    /// Configuración de política de firma XAdES EPES.
    /// </summary>
    public sealed class SignaturePolicySettings
    {
        /// <summary>
        /// OID o URN de la polÃ­tica. Si se deja vacÃ­o, se usarÃ¡ la polÃ­tica AEAT por defecto.
        /// </summary>
        public string Oid { get; set; } = string.Empty;

        /// <summary>
        /// URL pÃºblica de la polÃ­tica.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Hash base64 de la polÃ­tica.
        /// </summary>
        public string PolicyHashBase64 { get; set; } = string.Empty;

        /// <summary>
        /// Algoritmo del hash de la polÃ­tica: "SHA1" o "SHA256". Por defecto "SHA1" por compatibilidad AGE.
        /// </summary>
        public string HashAlgorithm { get; set; } = "SHA1";

        public bool IsConfigured() => !string.IsNullOrWhiteSpace(Oid) && !string.IsNullOrWhiteSpace(PolicyHashBase64);
    }
}
