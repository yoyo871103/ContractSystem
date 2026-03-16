using MediatR;

namespace ContractSystem.Application.Contratos.Commands.EliminarDocumentoAdjunto;

public record EliminarDocumentoAdjuntoCommand(int Id) : IRequest;

public class EliminarDocumentoAdjuntoCommandHandler : IRequestHandler<EliminarDocumentoAdjuntoCommand>
{
    private readonly IDocumentoAdjuntoStore _store;

    public EliminarDocumentoAdjuntoCommandHandler(IDocumentoAdjuntoStore store) => _store = store;

    public async Task Handle(EliminarDocumentoAdjuntoCommand request, CancellationToken cancellationToken)
    {
        var adjunto = await _store.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Documento adjunto no encontrado.");

        await _store.DeleteAsync(adjunto, cancellationToken);
    }
}
