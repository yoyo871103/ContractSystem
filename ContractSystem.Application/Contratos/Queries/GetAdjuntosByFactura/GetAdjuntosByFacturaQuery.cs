using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAdjuntosByFactura;

public record GetAdjuntosByFacturaQuery(int FacturaId) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetAdjuntosByFacturaQueryHandler : IRequestHandler<GetAdjuntosByFacturaQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetAdjuntosByFacturaQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetAdjuntosByFacturaQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByFacturaAsync(request.FacturaId, cancellationToken);
    }
}
