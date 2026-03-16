using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.AdjuntarDocumento;

public record AdjuntarDocumentoCommand(
    int ContratoId,
    string NombreArchivo,
    string Objetivo,
    byte[] Contenido) : IRequest<DocumentoAdjunto>;

public class AdjuntarDocumentoCommandHandler : IRequestHandler<AdjuntarDocumentoCommand, DocumentoAdjunto>
{
    private readonly IDocumentoAdjuntoStore _store;

    public AdjuntarDocumentoCommandHandler(IDocumentoAdjuntoStore store) => _store = store;

    public async Task<DocumentoAdjunto> Handle(AdjuntarDocumentoCommand request, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(request.NombreArchivo);

        if (string.IsNullOrEmpty(extension))
            throw new InvalidOperationException("El archivo no tiene extensión.");

        if (DocumentoAdjunto.ExtensionesProhibidas.Contains(extension))
            throw new InvalidOperationException($"El tipo de archivo '{extension}' no está permitido por seguridad.");

        if (!DocumentoAdjunto.ExtensionesPermitidas.Contains(extension))
            throw new InvalidOperationException(
                $"El tipo de archivo '{extension}' no está en la lista de tipos permitidos. " +
                $"Tipos permitidos: {string.Join(", ", DocumentoAdjunto.ExtensionesPermitidas)}");

        var adjunto = new DocumentoAdjunto
        {
            ContratoId = request.ContratoId,
            NombreArchivo = Path.GetFileName(request.NombreArchivo),
            Extension = extension.ToLowerInvariant(),
            Objetivo = request.Objetivo.Trim(),
            Contenido = request.Contenido,
            TamanioBytes = request.Contenido.LongLength,
            FechaCarga = DateTime.UtcNow
        };

        await _store.CreateAsync(adjunto, cancellationToken);
        return adjunto;
    }
}
