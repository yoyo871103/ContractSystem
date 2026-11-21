using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetDocumentoAdjuntoById;

/// <summary>Datos de un adjunto sin su contenido; null si no existe.</summary>
public record GetDocumentoAdjuntoByIdQuery(int Id) : IRequest<DocumentoAdjunto?>;

public class GetDocumentoAdjuntoByIdQueryHandler : IRequestHandler<GetDocumentoAdjuntoByIdQuery, DocumentoAdjunto?>
{
    private readonly IDocumentoAdjuntoStore _store;

    public GetDocumentoAdjuntoByIdQueryHandler(IDocumentoAdjuntoStore store) => _store = store;

    public Task<DocumentoAdjunto?> Handle(GetDocumentoAdjuntoByIdQuery request, CancellationToken cancellationToken) =>
        _store.GetByIdAsync(request.Id, cancellationToken);
}
