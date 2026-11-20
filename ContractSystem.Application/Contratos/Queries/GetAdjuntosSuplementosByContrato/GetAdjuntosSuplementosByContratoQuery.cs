using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAdjuntosSuplementosByContrato;

/// <summary>
/// Adjuntos generales (no de factura) de todos los suplementos hijos del contrato.
/// </summary>
public record GetAdjuntosSuplementosByContratoQuery(int ContratoPadreId) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetAdjuntosSuplementosByContratoQueryHandler : IRequestHandler<GetAdjuntosSuplementosByContratoQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetAdjuntosSuplementosByContratoQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetAdjuntosSuplementosByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetBySuplementosDeContratoAsync(request.ContratoPadreId, cancellationToken);
    }
}
