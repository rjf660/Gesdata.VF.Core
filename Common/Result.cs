using System.Collections.ObjectModel;

namespace Gesdata.VF.Core
{
    /// <summary>
    /// Patrón Result genérico para operaciones que pueden fallar.
    /// 
    /// NOTA: Este Result es diferente de VeriFactuResult:
    /// - Result<T>: Genérico, reutilizable en toda la capa Core
    /// - VeriFactuResult<T>: Específico de transporte SOAP con metadatos (SoapEnvelope, Duration, etc.)
    /// 
    /// Ambos coexisten porque tienen propósitos diferentes:
    /// - Result<T> → Validaciones XSD, parseo XML, operaciones genéricas
    /// - VeriFactuResult<T> → Llamadas SOAP a AEAT con auditoría completa
    /// </summary>
    public class Result
    {
        public bool Success { get; }

        public string ErrorMessage { get; } = string.Empty;

        public Exception Exception { get; }

        public IReadOnlyList<string> Errors { get; }

        protected Result(bool success, string errorMessage, Exception exception, IReadOnlyList<string> errors)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Exception = exception;
            Errors = errors;
        }

        public static Result Ok() => new(true, string.Empty, null, []);

        public static Result Fail(string message, Exception ex = null, IEnumerable<string> errors = null) => new(
            false,
            message,
            ex,
            new ReadOnlyCollection<string>(errors?.ToList() ?? []));
    }

    /// <summary>
    /// Resultado con valor tipado.
    /// </summary>
    /// <typeparam name="T">Tipo del valor en caso de éxito</typeparam>
    public sealed class Result<T> : Result
    {
        public T Value { get; }

        private Result(bool success, T value, string errorMessage, Exception exception, IReadOnlyList<string> errors) : base(
            success,
            errorMessage,
            exception,
            errors)
        { Value = value; }

        public static Result<T> Ok(T value) => new(true, value, string.Empty, null, []);

        public static new Result<T> Fail(string message, Exception ex = null, IEnumerable<string> errors = null) => new(
            false,
            default,
            message,
            ex,
            new ReadOnlyCollection<string>(errors?.ToList() ?? []));
    }
}
