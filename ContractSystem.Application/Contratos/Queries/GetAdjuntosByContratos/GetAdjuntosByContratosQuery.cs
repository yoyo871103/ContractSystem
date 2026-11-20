using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAdjuntosByContratos;

/// <summary>
/// Todos los adjuntos (del contrato, de sus facturas y de sus anexos) de los contratos indicados.
/// Se usa para la vista unificada de adjuntos de un contrato y sus suplementos.
/// </summary>
public record GetAdjuntosByContratosQuery(IReadOnlyCollection<int> ContratoIds) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetAdjuntosByContratosQueryHandler : IRequestHandler<GetAdjuntosByContratosQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetAdjuntosByContratosQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetAdjuntosByContratosQuery request, CancellationToken cancellationToken)
        => _store.GetByContratosAsync(request.ContratoIds, cancellationToken);
}
