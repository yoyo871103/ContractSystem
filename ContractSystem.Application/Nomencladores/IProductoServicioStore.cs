using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

public interface IProductoServicioStore
{
    Task<ProductoServicio?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductoServicio>> GetAllAsync(bool includeDeleted = false, TipoProductoServicio? tipo = null, CancellationToken cancellationToken = default);
    Task<ProductoServicio> CreateAsync(ProductoServicio entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductoServicio entity, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UndeleteAsync(int id, CancellationToken cancellationToken = default);
}
