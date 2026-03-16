using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IDocumentoAdjuntoStore
{
    Task<IReadOnlyList<DocumentoAdjunto>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default);
    Task<DocumentoAdjunto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DocumentoAdjunto?> GetByIdConContenidoAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default);
    Task DeleteAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default);
}
