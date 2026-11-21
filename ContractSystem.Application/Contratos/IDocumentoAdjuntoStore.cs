using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IDocumentoAdjuntoStore
{
    /// <summary>
    /// Adjuntos generales del contrato (sin factura ni anexo).
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjuntos de una factura concreta.
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetByFacturaAsync(int facturaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjuntos generales (FacturaId == null) de todos los suplementos hijos del contrato indicado.
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetBySuplementosDeContratoAsync(int contratoPadreId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjuntos de todas las facturas del contrato indicado.
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetByFacturasDeContratoAsync(int contratoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjuntos de un anexo concreto.
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetByAnexoAsync(int anexoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todos los adjuntos (generales, de facturas y de anexos) de los contratos indicados.
    /// </summary>
    Task<IReadOnlyList<DocumentoAdjunto>> GetByContratosAsync(IReadOnlyCollection<int> contratoIds, CancellationToken cancellationToken = default);

    Task<DocumentoAdjunto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DocumentoAdjunto?> GetByIdConContenidoAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default);

    /// <summary>Sustituye el archivo (contenido, tamaño y fecha de carga). false si el adjunto no existe.</summary>
    Task<bool> ReemplazarContenidoAsync(int id, byte[] contenido, CancellationToken cancellationToken = default);
    Task DeleteAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default);
}
