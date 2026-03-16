using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

/// <summary>
/// Servicio de generación de números de documento según la configuración activa.
/// </summary>
public interface IDocumentoNumeracionService
{
    /// <summary>
    /// Genera el siguiente número de documento resolviendo variables del formato configurado.
    /// </summary>
    Task<string> GenerarNumeroAsync(
        TipoDocumentoContrato tipo,
        string? codigoTercero = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera una vista previa del formato sin incrementar el contador.
    /// </summary>
    Task<string> VistaPrevia(
        string formato,
        int digitosPadding,
        bool contadorPorAnio,
        CancellationToken cancellationToken = default);
}
