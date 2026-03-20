namespace Gesdata.VF.Core.Exceptions
{
    /// <summary>
    /// Excepción base para errores relacionados con la AEAT.
    /// </summary>
    public class AeatException : Exception
    {
        public string CodigoError { get; }
        public string DescripcionError { get; }

        public AeatException(string mensaje) : base(mensaje)
        {
        }

        public AeatException(string mensaje, Exception innerException)
            : base(mensaje, innerException)
        {
        }

        public AeatException(string codigoError, string descripcionError, string mensaje)
            : base(mensaje)
        {
            CodigoError = codigoError;
            DescripcionError = descripcionError;
        }

        public AeatException(string codigoError, string descripcionError, string mensaje, Exception innerException)
            : base(mensaje, innerException)
        {
            CodigoError = codigoError;
            DescripcionError = descripcionError;
        }
    }

    /// <summary>
    /// Excepción para errores transitorios de la AEAT que pueden resolverse con reintentos.
    /// Ejemplos: timeouts, errores de red, servidor ocupado (503), etc.
    /// </summary>
    public class AeatTransientException : AeatException
    {
        public bool EsReintentable { get; }
        public TimeSpan? TiempoEsperaRecomendado { get; set; } // ✅ CAMBIADO: De solo lectura a mutable

        public AeatTransientException(string mensaje)
            : base(mensaje)
        {
            EsReintentable = true;
        }

        public AeatTransientException(string mensaje, TimeSpan tiempoEsperaRecomendado)
            : base(mensaje)
        {
            EsReintentable = true;
            TiempoEsperaRecomendado = tiempoEsperaRecomendado;
        }

        public AeatTransientException(string codigoError, string descripcionError, string mensaje)
            : base(codigoError, descripcionError, mensaje)
        {
            EsReintentable = true;
        }

        public AeatTransientException(string codigoError, string descripcionError, string mensaje, Exception innerException)
            : base(codigoError, descripcionError, mensaje, innerException)
        {
            EsReintentable = true;
        }
    }

    /// <summary>
    /// Excepción para errores permanentes de la AEAT que NO se pueden resolver con reintentos.
    /// Ejemplos: certificado inválido, datos incorrectos, factura duplicada, etc.
    /// </summary>
    public class AeatPermanentException : AeatException
    {
        public AeatPermanentException(string mensaje)
            : base(mensaje)
        {
        }

        public AeatPermanentException(string codigoError, string descripcionError, string mensaje)
            : base(codigoError, descripcionError, mensaje)
        {
        }

        public AeatPermanentException(string codigoError, string descripcionError, string mensaje, Exception innerException)
            : base(codigoError, descripcionError, mensaje, innerException)
        {
        }
    }
}
