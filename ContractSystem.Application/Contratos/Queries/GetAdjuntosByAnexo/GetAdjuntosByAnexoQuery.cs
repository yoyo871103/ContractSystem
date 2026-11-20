using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAdjuntosByAnexo;

public record GetAdjuntosByAnexoQuery(int AnexoId) : IRequest<IReadOnlyList<DocumentoAdjunto>>;

public class GetAdjuntosByAnexoQueryHandler : IRequestHandler<GetAdjuntosByAnexoQuery, IReadOnlyList<DocumentoAdjunto>>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetAdjuntosByAnexoQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<IReadOnlyList<DocumentoAdjunto>> Handle(GetAdjuntosByAnexoQuery request, CancellationToken cancellationToken)
        => _store.GetByAnexoAsync(request.AnexoId, cancellationToken);
}
