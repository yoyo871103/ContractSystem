using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.DeleteProductoServicio;

public record DeleteProductoServicioCommand(int Id) : IRequest<Unit>;

public class DeleteProductoServicioCommandHandler : IRequestHandler<DeleteProductoServicioCommand, Unit>
{
    private readonly IProductoServicioStore _store;

    public DeleteProductoServicioCommandHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(DeleteProductoServicioCommand request, CancellationToken cancellationToken)
    {
        await _store.SoftDeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
