using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IHistorialCambioStore
{
    Task<IReadOnlyList<HistorialCambio>> GetByContratoAsync(int contratoId, TipoCambio? filtroTipo = null, CancellationToken cancellationToken = default);
    Task CreateAsync(HistorialCambio historial, CancellationToken cancellationToken = default);
}
