using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

public interface IUnidadMedidaStore
{
    Task<UnidadMedida?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnidadMedida>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<UnidadMedida> CreateAsync(UnidadMedida unidadMedida, CancellationToken cancellationToken = default);
    Task UpdateAsync(UnidadMedida unidadMedida, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UndeleteAsync(int id, CancellationToken cancellationToken = default);
}
