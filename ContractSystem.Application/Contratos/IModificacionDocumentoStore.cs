using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IModificacionDocumentoStore
{
    /// <summary>
    /// Obtiene todas las relaciones donde el documento dado modifica a otros ("modifica a").
    /// </summary>
    Task<IReadOnlyList<ModificacionDocumento>> GetModificaAAsync(int documentoOrigenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las relaciones donde el documento dado es modificado por otros ("modificado por").
    /// </summary>
    Task<IReadOnlyList<ModificacionDocumento>> GetModificadoPorAsync(int documentoDestinoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una relación directa o inversa entre dos documentos (para prevenir ciclos R04).
    /// </summary>
    Task<bool> ExisteRelacionAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default);

    Task<ModificacionDocumento> CreateAsync(ModificacionDocumento modificacion, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
