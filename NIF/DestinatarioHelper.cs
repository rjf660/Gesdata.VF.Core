using System.Text.RegularExpressions;
using Gesdata.Comun.NIF; // ← CAMBIO: Core compartido
using Gesdata.Comun.NIF.VIES; // ← CAMBIO: VIES compartido
using Gesdata.VF.Contracts.EnumTypes;
using Gesdata.VF.Contracts.Types;

namespace Gesdata.VF.Core.NIF
{
    /// <summary>
    /// UtilComun para configurar destinatarios en facturas VERI*FACTU. Previene errores 4118 de AEAT relacionados con
    /// validación de NIF/identificación.
    /// </summary>
    public static class DestinatarioHelper
    {
        /// <summary>
        /// Crea un destinatario español con NIF validado.
        /// </summary>
        /// <param name="nif">NIF español (9 caracteres).</param>
        /// <param name="nombreRazon">Nombre o razón social del destinatario.</param>
        /// <param name="validarDigitoControl">Si true, valida el dígito de control del NIF.</param>
        /// <returns>Objeto PersonaFisicaJuridicaType configurado para destinatario español.</returns>
        /// <exception cref="ArgumentException">Si NIF o nombreRazon están vacíos o el NIF es inválido.</exception>
        public static PersonaFisicaJuridicaType CrearDestinatarioEspanol(
            string nif,
            string nombreRazon,
            bool validarDigitoControl = true)
        {
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF no puede estar vacío.", nameof(nif));

            if (string.IsNullOrWhiteSpace(nombreRazon))
                throw new ArgumentException("Nombre/Razón no puede estar vacío.", nameof(nombreRazon));

            // Normalizar NIF usando el core compartido
            nif = NifValidatorCore.NormalizarNif(nif);

            // Validar dígito de control si se solicita
            if (validarDigitoControl && !NifValidatorCore.ValidarNifEspanol(nif))
            {
                var tipo = NifValidatorCore.DetectarTipo(nif);
                throw new ArgumentException(
                    $"NIF '{nif}' no es válido. Tipo detectado: {tipo}. Verifique el dígito de control.",
                    nameof(nif));
            }
            else if (!validarDigitoControl && !NifValidatorCore.ValidarFormatoBasico(nif))
            {
                throw new ArgumentException(
                    $"NIF '{nif}' no tiene formato válido (debe tener 9 caracteres alfanuméricos).",
                    nameof(nif));
            }

            return new PersonaFisicaJuridicaType
            {
                NIF = nif,
                NombreRazon = nombreRazon.Trim(),
                IDOtro = null // Asegurar que está vacío
            };
        }

        /// <summary>
        /// Crea un destinatario extranjero con IDOtro.
        /// </summary>
        /// <param name="codigoPais">Código ISO de país (2 letras, excepto ES).</param>
        /// <param name="id">Número de identificación fiscal del país.</param>
        /// <param name="nombreRazon">Nombre o razón social del destinatario.</param>
        /// <param name="tipoId">Tipo de identificación (por defecto NIF_IVA).</param>
        /// <param name="validarEstructuraVies">Si true, valida la estructura según patrones VIES.</param>
        /// <returns>Objeto PersonaFisicaJuridicaType configurado para destinatario extranjero.</returns>
        /// <exception cref="ArgumentException">Si algún parámetro requerido está vacío o es inválido.</exception>
        public static PersonaFisicaJuridicaType CrearDestinatarioExtranjero(
            string codigoPais,
            string id,
            string nombreRazon,
            IDType tipoId = IDType.NIF_IVA,
            bool validarEstructuraVies = true)
        {
            if (string.IsNullOrWhiteSpace(codigoPais))
                throw new ArgumentException("Código país no puede estar vacío.", nameof(codigoPais));

            codigoPais = codigoPais.Trim().ToUpperInvariant();

            if (codigoPais == "ES")
                throw new ArgumentException(
                     "Para España use CrearDestinatarioEspanol. CodigoPais 'ES' no es válido para IDOtro.",
                     nameof(codigoPais));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID no puede estar vacío.", nameof(id));

            if (string.IsNullOrWhiteSpace(nombreRazon))
                throw new ArgumentException("Nombre/Razón no puede estar vacío.", nameof(nombreRazon));

            // Validar estructura VIES si se solicita y el país está en la UE
            if (validarEstructuraVies && ViesVatNumber.PatternsByCountry.ContainsKey(codigoPais))
            {
                var vatNumber = codigoPais + id.Trim();
                if (!ViesVatNumber.ValidateStructure(vatNumber))
                {
                    throw new ArgumentException($"ID '{id}' no tiene estructura válida para país {codigoPais} según VIES.",
                       nameof(id));
                }
            }

            return new PersonaFisicaJuridicaType
            {
                NIF = string.Empty, // Vacío para extranjeros
                NombreRazon = nombreRazon.Trim(),
                IDOtro = new IDOtroType { CodigoPais = codigoPais, IDType = tipoId, ID = id.Trim() }
            };
        }

        /// <summary>
        /// Crea un destinatario extranjero con validación VIES online (asíncrono).
        /// </summary>
        public static async Task<PersonaFisicaJuridicaType> CrearDestinatarioExtranjeroValidadoAsync(
            string codigoPais,
            string id,
            string nombreRazon,
            IDType tipoId = IDType.NIF_IVA,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(codigoPais))
                throw new ArgumentException("Código país no puede estar vacío.", nameof(codigoPais));

            codigoPais = codigoPais.Trim().ToUpperInvariant();

            if (codigoPais == "ES")
                throw new ArgumentException("Para España use CrearDestinatarioEspanol.", nameof(codigoPais));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID no puede estar vacío.", nameof(id));

            // Validar con VIES online
            var validator = new NifValidator();
            var resultado = await validator.ValidarAsync(codigoPais, id, ct).ConfigureAwait(false);

            if (!resultado.Success)
            {
                throw new ArgumentException(
                     $"ID '{id}' no es válido para {codigoPais}: {resultado.ErrorMessage}",
                       nameof(id));
            }

            // Usar nombre registrado en VIES si está disponible
            var nombreFinal = !string.IsNullOrWhiteSpace(resultado.NombreRegistrado)
                ? resultado.NombreRegistrado
                : nombreRazon?.Trim();

            return string.IsNullOrWhiteSpace(nombreFinal)
                ? throw new ArgumentException("Nombre/Razón no puede estar vacío.", nameof(nombreRazon))
                : new PersonaFisicaJuridicaType
                {
                    NIF = string.Empty,
                    NombreRazon = nombreFinal,
                    IDOtro = new IDOtroType { CodigoPais = codigoPais, IDType = tipoId, ID = id.Trim() }
                };
        }

        /// <summary>
        /// Normaliza el nombre/razón social eliminando espacios múltiples y aplicando formato. Útil para coincidir con
        /// los datos registrados en AEAT.
        /// </summary>
        /// <param name="nombreRazon">Nombre o razón social a normalizar.</param>
        /// <param name="convertirMayusculas">Si es true, convierte a mayúsculas (recomendado para AEAT).</param>
        /// <returns>Nombre normalizado.</returns>
        public static string NormalizarNombreRazon(string nombreRazon, bool convertirMayusculas = true)
        {
            if (string.IsNullOrWhiteSpace(nombreRazon))
                return string.Empty;

            // Eliminar espacios múltiples
            var normalized = Regex.Replace(nombreRazon.Trim(), @"\s+", " ");

            // Convertir a mayúsculas si se solicita (AEAT suele usar mayúsculas)
            return convertirMayusculas ? normalized.ToUpperInvariant() : normalized;
        }
    }
}
