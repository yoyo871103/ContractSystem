using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAdjuntosFacturasByContrato;

/// <summary>
/// Adjuntos de todas las facturas del contrato indicado.
/// </summary>
public record GetAdjuntosFacturasByContratoQuery(int ContratoId) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetAdjuntosFacturasByContratoQueryHandler : IRequestHandler<GetAdjuntosFacturasByContratoQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetAdjuntosFacturasByContratoQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetAdjuntosFacturasByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByFacturasDeContratoAsync(request.ContratoId, cancellationToken);
    }
}
