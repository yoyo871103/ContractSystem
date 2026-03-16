using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IAnexoStore
{
    Task<IReadOnlyList<Anexo>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default);
    Task<Anexo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Anexo> CreateAsync(Anexo anexo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Anexo anexo, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
