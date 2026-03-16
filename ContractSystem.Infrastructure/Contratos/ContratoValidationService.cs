using ContractSystem.Application.Contratos;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class ContratoValidationService : IContratoValidationService
{
    private readonly IContratoStore _contratoStore;
    private readonly IModificacionDocumentoStore _modificacionStore;

    public ContratoValidationService(IContratoStore contratoStore, IModificacionDocumentoStore modificacionStore)
    {
        _contratoStore = contratoStore;
        _modificacionStore = modificacionStore;
    }

    /// <summary>
    /// R03: Para establecer que A modifica a B, B debe tener fecha de firma definida
    /// y la fecha de firma de A debe ser igual o posterior a la fecha de firma de B.
    /// </summary>
    public async Task<ValidationResult> ValidarFechasModificacionAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default)
    {
        var origen = await _contratoStore.GetByIdAsync(documentoOrigenId, cancellationToken: cancellationToken);
        var destino = await _contratoStore.GetByIdAsync(documentoDestinoId, cancellationToken: cancellationToken);

        if (origen is null)
            return ValidationResult.Error("El documento origen no existe.");

        if (destino is null)
            return ValidationResult.Error("El documento destino no existe.");

        if (!destino.FechaFirma.HasValue)
            return ValidationResult.Error($"El documento '{destino.Numero}' no tiene fecha de firma definida. Es necesaria para establecer la relación de modificación.");

        if (!origen.FechaFirma.HasValue)
            return ValidationResult.Error($"El documento '{origen.Numero}' no tiene fecha de firma definida.");

        if (origen.FechaFirma.Value < destino.FechaFirma.Value)
            return ValidationResult.Error(
                $"La fecha de firma del documento origen ({origen.FechaFirma.Value:dd/MM/yyyy}) " +
                $"es anterior a la del documento destino ({destino.FechaFirma.Value:dd/MM/yyyy}). " +
                "La fecha de firma del documento que modifica debe ser igual o posterior.");

        return ValidationResult.Ok();
    }

    /// <summary>
    /// R04: Prevención de ciclos. Verifica que no exista ya una relación inversa (B→A) ni directa (A→B).
    /// </summary>
    public async Task<ValidationResult> ValidarSinCiclosAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default)
    {
        if (documentoOrigenId == documentoDestinoId)
            return ValidationResult.Error("Un documento no puede modificarse a sí mismo.");

        var existeRelacion = await _modificacionStore.ExisteRelacionAsync(documentoOrigenId, documentoDestinoId, cancellationToken);
        if (existeRelacion)
            return ValidationResult.Error("Ya existe una relación de modificación entre estos documentos (directa o inversa). No se permiten relaciones circulares.");

        return ValidationResult.Ok();
    }

    /// <summary>
    /// Valida que el número de documento no esté vacío.
    /// Los números pueden repetirse (ej: suplementos de distintos padres con el mismo número).
    /// </summary>
    public Task<ValidationResult> ValidarNumeroUnicoAsync(string numero, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return Task.FromResult(ValidationResult.Error("El número de documento es obligatorio."));

        return Task.FromResult(ValidationResult.Ok());
    }

    /// <summary>
    /// R09: Suplementos de Contrato Marco solo pueden ser para modificaciones de generales,
    /// no de servicios/productos.
    /// </summary>
    public ValidationResult ValidarSuplementoMarco(TipoDocumentoContrato tipoPadre, bool esModificacionGenerales)
    {
        if (tipoPadre == TipoDocumentoContrato.Marco && !esModificacionGenerales)
            return ValidationResult.Error(
                "Los suplementos de un Contrato Marco solo pueden ser para modificaciones de condiciones generales. " +
                "Para modificar servicios/productos, cree el suplemento desde un Contrato Específico.");

        return ValidationResult.Ok();
    }
}
