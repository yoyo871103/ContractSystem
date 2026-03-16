using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.AddModificacion;

public record AddModificacionCommand(
    int DocumentoOrigenId,
    int DocumentoDestinoId,
    string Descripcion) : IRequest<ModificacionDocumento>;

public class AddModificacionCommandHandler : IRequestHandler<AddModificacionCommand, ModificacionDocumento>
{
    private readonly IModificacionDocumentoStore _store;
    private readonly IContratoValidationService _validation;

    public AddModificacionCommandHandler(IModificacionDocumentoStore store, IContratoValidationService validation)
    {
        _store = store;
        _validation = validation;
    }

    public async Task<ModificacionDocumento> Handle(AddModificacionCommand request, CancellationToken cancellationToken)
    {
        // R03: Validar fechas
        var r03 = await _validation.ValidarFechasModificacionAsync(request.DocumentoOrigenId, request.DocumentoDestinoId, cancellationToken);
        if (!r03.EsValido)
            throw new InvalidOperationException(r03.MensajeError);

        // R04: Validar ciclos
        var r04 = await _validation.ValidarSinCiclosAsync(request.DocumentoOrigenId, request.DocumentoDestinoId, cancellationToken);
        if (!r04.EsValido)
            throw new InvalidOperationException(r04.MensajeError);

        var modificacion = new ModificacionDocumento
        {
            DocumentoOrigenId = request.DocumentoOrigenId,
            DocumentoDestinoId = request.DocumentoDestinoId,
            Descripcion = request.Descripcion?.Trim() ?? string.Empty
        };

        return await _store.CreateAsync(modificacion, cancellationToken);
    }
}
