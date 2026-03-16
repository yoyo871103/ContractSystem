using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.DescargarDocumentoAdjunto;

public record DescargarDocumentoAdjuntoQuery(int Id) : IRequest<DocumentoAdjunto>;

public class DescargarDocumentoAdjuntoQueryHandler : IRequestHandler<DescargarDocumentoAdjuntoQuery, DocumentoAdjunto>
{
    private readonly IDocumentoAdjuntoStore _store;

    public DescargarDocumentoAdjuntoQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public async Task<DocumentoAdjunto> Handle(DescargarDocumentoAdjuntoQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetByIdConContenidoAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Documento adjunto no encontrado.");
    }
}
