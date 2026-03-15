using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.ReactivarUnidadMedida;

public record ReactivarUnidadMedidaCommand(int Id) : IRequest<Unit>;

public class ReactivarUnidadMedidaCommandHandler : IRequestHandler<ReactivarUnidadMedidaCommand, Unit>
{
    private readonly IUnidadMedidaStore _store;

    public ReactivarUnidadMedidaCommandHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(ReactivarUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        await _store.UndeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
