using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface ILineaDetalleStore
{
    Task<IReadOnlyList<LineaDetalle>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LineaDetalle>> GetByAnexoAsync(int anexoId, CancellationToken cancellationToken = default);
    Task<LineaDetalle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LineaDetalle> CreateAsync(LineaDetalle linea, CancellationToken cancellationToken = default);
    Task UpdateAsync(LineaDetalle linea, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
