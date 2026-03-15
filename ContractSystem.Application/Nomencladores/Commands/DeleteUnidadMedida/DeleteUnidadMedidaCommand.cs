using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.DeleteUnidadMedida;

public record DeleteUnidadMedidaCommand(int Id) : IRequest<Unit>;

public class DeleteUnidadMedidaCommandHandler : IRequestHandler<DeleteUnidadMedidaCommand, Unit>
{
    private readonly IUnidadMedidaStore _store;

    public DeleteUnidadMedidaCommandHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(DeleteUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        await _store.SoftDeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
