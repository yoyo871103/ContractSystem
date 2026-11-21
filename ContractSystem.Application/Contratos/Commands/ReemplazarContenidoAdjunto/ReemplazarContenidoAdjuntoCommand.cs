using MediatR;

namespace ContractSystem.Application.Contratos.Commands.ReemplazarContenidoAdjunto;

/// <summary>
/// Sustituye el archivo de un adjunto por una nueva versión (p. ej. la proforma ya llenada en Word),
/// conservando su asociación (contrato, factura o anexo) y su objetivo.
/// </summary>
public record ReemplazarContenidoAdjuntoCommand(int Id, byte[] Contenido) : IRequest;

public class ReemplazarContenidoAdjuntoCommandHandler : IRequestHandler<ReemplazarContenidoAdjuntoCommand>
{
    private readonly IDocumentoAdjuntoStore _store;

    public ReemplazarContenidoAdjuntoCommandHandler(IDocumentoAdjuntoStore store) => _store = store;

    public async Task Handle(ReemplazarContenidoAdjuntoCommand request, CancellationToken cancellationToken)
    {
        if (request.Contenido.Length == 0)
            throw new InvalidOperationException("El archivo está vacío.");

        if (!await _store.ReemplazarContenidoAsync(request.Id, request.Contenido, cancellationToken))
            throw new InvalidOperationException("Documento adjunto no encontrado (puede que lo hayan eliminado).");
    }
}
