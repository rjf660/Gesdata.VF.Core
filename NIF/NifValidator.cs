using Gesdata.Comun.NIF; // ← CAMBIO: Core compartido desde Gesdata.Comun
using Gesdata.Comun.NIF.VIES; // ← CAMBIO: VIES desde Gesdata.Comun
using Gesdata.VF.Contracts.Types;

namespace Gesdata.VF.Core.NIF
{
    /// <summary>
    /// Validador con capacidad async para integración con VIES.
    /// Específico para VERI*FACTU, usa Gesdata.Comun.NIF como motor.
    /// </summary>
    public class NifValidator
    {
        /// <summary>
        /// Valida un identificador según el país, integrando validación VIES para UE.
        /// </summary>
        public async Task<NifValidationResult> ValidarAsync(
            string codigoPais,
            string identificador,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(codigoPais))
            {
                return new NifValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Código de país vacío."
                };
            }

            codigoPais = codigoPais.Trim().ToUpperInvariant();
            identificador = NifValidatorCore.NormalizarNif(identificador);

            // España - validación NIF/NIE/CIF local
            if (codigoPais == "ES")
            {
                var esValido = NifValidatorCore.ValidarNifEspanol(identificador);
                var tipo = NifValidatorCore.DetectarTipo(identificador);

                return new NifValidationResult
                {
                    IsValid = esValido,
                    CodigoPais = "ES",
                    Identificador = identificador,
                    TipoValidacion = $"NIF Español ({tipo})",
                    ErrorMessage = esValido ? null : $"NIF español inválido. Tipo detectado: {tipo}."
                };
            }

            // Países UE - validar con VIES si está disponible
            if (ViesVatNumber.PatternsByCountry.ContainsKey(codigoPais))
            {
                var vatNumber = codigoPais + identificador;

                // Primero validar estructura localmente
                if (!ViesVatNumber.ValidateStructure(vatNumber))
                {
                    return new NifValidationResult
                    {
                        IsValid = false,
                        CodigoPais = codigoPais,
                        Identificador = identificador,
                        TipoValidacion = "VIES (Estructura)",
                        ErrorMessage = $"Estructura de IVA inválida para {codigoPais}."
                    };
                }

                // Validar con VIES (puede fallar por timeout/red)
                try
                {
                    var viesResult = await ViesVatNumber.ValidateDetailedAsync(vatNumber, ct).ConfigureAwait(false);

                    return new NifValidationResult
                    {
                        IsValid = viesResult.IsValid,
                        CodigoPais = codigoPais,
                        Identificador = identificador,
                        NombreRegistrado = viesResult.Name,
                        DireccionRegistrada = viesResult.Address,
                        TipoValidacion = "VIES (Online)",
                        ErrorMessage = viesResult.ErrorMessage
                    };
                }
                catch
                {
                    // Estructura válida pero VIES no disponible
                    return new NifValidationResult
                    {
                        IsValid = true, // Estructura válida
                        CodigoPais = codigoPais,
                        Identificador = identificador,
                        TipoValidacion = "VIES (Solo estructura)",
                        ErrorMessage = "Estructura válida. VIES no disponible."
                    };
                }
            }

            // País no-UE - solo validar que no esté vacío
            return new NifValidationResult
            {
                IsValid = !string.IsNullOrWhiteSpace(identificador),
                CodigoPais = codigoPais,
                Identificador = identificador,
                TipoValidacion = "País no-UE (Solo formato)",
                ErrorMessage = string.IsNullOrWhiteSpace(identificador)
                    ? "Identificador vacío para país no-UE."
                    : null
            };
        }

        /// <summary>
        /// Valida un destinatario de VERI*FACTU completo.
        /// </summary>
        public async Task<NifValidationResult> ValidarDestinatarioAsync(
            PersonaFisicaJuridicaType destinatario,
            bool validarVies = false,
            CancellationToken ct = default)
        {
            if (destinatario == null)
            {
                return new NifValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Destinatario nulo."
                };
            }

            var hasNif = !string.IsNullOrWhiteSpace(destinatario.NIF);
            var hasIdOtro = destinatario.IDOtro != null &&
                !string.IsNullOrWhiteSpace(destinatario.IDOtro.ID);

            // Validaciones básicas
            if (!hasNif && !hasIdOtro)
            {
                return new NifValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Destinatario '{destinatario.NombreRazon}' no tiene NIF ni IDOtro."
                };
            }

            if (hasNif && hasIdOtro)
            {
                return new NifValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Destinatario '{destinatario.NombreRazon}' tiene NIF e IDOtro (debe tener solo uno)."
                };
            }

            // Validar NIF español
            if (hasNif)
            {
                return await ValidarAsync("ES", destinatario.NIF, ct).ConfigureAwait(false);
            }

            // Validar IDOtro extranjero
            if (hasIdOtro)
            {
                if (string.IsNullOrWhiteSpace(destinatario.IDOtro?.CodigoPais))
                {
                    return new NifValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "IDOtro sin código de país."
                    };
                }

                if (destinatario.IDOtro.CodigoPais.Equals("ES", StringComparison.OrdinalIgnoreCase))
                {
                    return new NifValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "IDOtro con CodigoPais='ES' debe usar NIF en su lugar."
                    };
                }

                // Solo validar VIES si se solicita explícitamente
                return !validarVies
                    ? new NifValidationResult
                    {
                        IsValid = true,
                        CodigoPais = destinatario.IDOtro.CodigoPais,
                        Identificador = destinatario.IDOtro.ID,
                        TipoValidacion = "IDOtro (Sin validación VIES)",
                        ErrorMessage = null
                    }
                    : await ValidarAsync(
                    destinatario.IDOtro.CodigoPais,
                    destinatario.IDOtro.ID,
                    ct).ConfigureAwait(false);
            }

            return new NifValidationResult
            {
                IsValid = false,
                ErrorMessage = "Estado inconsistente del destinatario."
            };
        }
    }

    /// <summary>
    /// Resultado de validación de NIF/identificador.
    /// </summary>
    public class NifValidationResult
    {
        public bool IsValid { get; set; }
        public string CodigoPais { get; set; }
        public string Identificador { get; set; }
        public string NombreRegistrado { get; set; }
        public string DireccionRegistrada { get; set; }
        public string TipoValidacion { get; set; }
        public string ErrorMessage { get; set; }

        public bool Success => IsValid && string.IsNullOrWhiteSpace(ErrorMessage);
    }
}
