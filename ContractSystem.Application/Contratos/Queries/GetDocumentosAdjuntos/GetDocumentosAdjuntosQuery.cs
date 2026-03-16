using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetDocumentosAdjuntos;

public record GetDocumentosAdjuntosQuery(int ContratoId) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetDocumentosAdjuntosQueryHandler : IRequestHandler<GetDocumentosAdjuntosQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetDocumentosAdjuntosQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetDocumentosAdjuntosQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByContratoAsync(request.ContratoId, cancellationToken);
    }
}
