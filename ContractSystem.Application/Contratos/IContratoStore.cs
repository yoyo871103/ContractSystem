using ContractSystem.Application.Common.Models;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IContratoStore
{
    Task<Contrato?> GetByIdAsync(int id, bool includeRelaciones = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contrato>> GetAllAsync(
        bool includeDeleted = false,
        TipoDocumentoContrato? tipo = null,
        EstadoContrato? estado = null,
        RolContrato? rol = null,
        int? terceroId = null,
        DateTime? fechaFirmaDesde = null,
        DateTime? fechaFirmaHasta = null,
        string? textoBusqueda = null,
        string? textoTercero = null,
        string? textoProductoServicio = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene contratos Marco disponibles para ser padre de un Específico.
    /// </summary>
    Task<IReadOnlyList<Contrato>> GetContratosMarcoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los hijos directos (Específicos o Suplementos) de un contrato.
    /// </summary>
    Task<IReadOnlyList<Contrato>> GetHijosAsync(int contratoPadreId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un número de documento ya existe (para validar R06).
    /// </summary>
    Task<bool> ExisteNumeroAsync(string numero, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<PagedList<Contrato>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        bool includeDeleted = false,
        TipoDocumentoContrato? tipo = null,
        EstadoContrato? estado = null,
        RolContrato? rol = null,
        int? terceroId = null,
        string? textoBusqueda = null,
        string? textoTercero = null,
        CancellationToken cancellationToken = default);

    Task<Contrato> CreateAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task UpdateAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
