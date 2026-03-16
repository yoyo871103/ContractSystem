using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

/// <summary>
/// Servicio de validación de reglas de negocio para contratos y suplementos.
/// </summary>
public interface IContratoValidationService
{
    /// <summary>
    /// R03: Valida que fecha_firma(A) >= fecha_firma(B) para crear relación "modifica a".
    /// </summary>
    Task<ValidationResult> ValidarFechasModificacionAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// R04: Verifica que no exista una relación circular (A→B y B→A).
    /// </summary>
    Task<ValidationResult> ValidarSinCiclosAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// R06: Verifica que el número de documento sea único.
    /// </summary>
    Task<ValidationResult> ValidarNumeroUnicoAsync(string numero, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// R09: Valida que suplementos de Marco solo sean para modificaciones generales.
    /// </summary>
    ValidationResult ValidarSuplementoMarco(TipoDocumentoContrato tipoPadre, bool esModificacionGenerales);
}

/// <summary>
/// Resultado de una validación de regla de negocio.
/// </summary>
public record ValidationResult(bool EsValido, string? MensajeError = null)
{
    public static ValidationResult Ok() => new(true);
    public static ValidationResult Error(string mensaje) => new(false, mensaje);
}
