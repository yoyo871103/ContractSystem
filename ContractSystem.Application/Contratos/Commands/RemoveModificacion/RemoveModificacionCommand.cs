using MediatR;

namespace ContractSystem.Application.Contratos.Commands.RemoveModificacion;

public record RemoveModificacionCommand(int Id) : IRequest<Unit>;

public class RemoveModificacionCommandHandler : IRequestHandler<RemoveModificacionCommand, Unit>
{
    private readonly IModificacionDocumentoStore _store;

    public RemoveModificacionCommandHandler(IModificacionDocumentoStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(RemoveModificacionCommand request, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
