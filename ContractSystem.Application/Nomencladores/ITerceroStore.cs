using ContractSystem.Application.Common.Models;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

public interface ITerceroStore
{
    Task<Tercero?> GetByIdAsync(int id, bool includeContactos = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tercero>> GetAllAsync(bool includeDeleted = false, TipoTercero? tipo = null, CancellationToken cancellationToken = default);
    Task<PagedList<Tercero>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false, TipoTercero? tipo = null, string? searchText = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tercero>> SearchAsync(string texto, int maxResults = 20, CancellationToken cancellationToken = default);
    Task<Tercero> CreateAsync(Tercero tercero, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tercero tercero, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UndeleteAsync(int id, CancellationToken cancellationToken = default);
}
