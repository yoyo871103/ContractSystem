using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.ReactivarProductoServicio;

public record ReactivarProductoServicioCommand(int Id) : IRequest<Unit>;

public class ReactivarProductoServicioCommandHandler : IRequestHandler<ReactivarProductoServicioCommand, Unit>
{
    private readonly IProductoServicioStore _store;

    public ReactivarProductoServicioCommandHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(ReactivarProductoServicioCommand request, CancellationToken cancellationToken)
    {
        await _store.UndeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
