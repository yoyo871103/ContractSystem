using MediatR;

namespace ContractSystem.Application.Contratos.Commands.DeleteLineaDetalle;

public record DeleteLineaDetalleCommand(int Id) : IRequest<Unit>;

public class DeleteLineaDetalleCommandHandler : IRequestHandler<DeleteLineaDetalleCommand, Unit>
{
    private readonly ILineaDetalleStore _store;

    public DeleteLineaDetalleCommandHandler(ILineaDetalleStore store) => _store = store;

    public async Task<Unit> Handle(DeleteLineaDetalleCommand request, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
