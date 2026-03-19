using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IFacturaStore
{
    Task<IReadOnlyList<Factura>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default);
    Task<Factura?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(Factura factura, CancellationToken cancellationToken = default);
    Task UpdateAsync(Factura factura, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
