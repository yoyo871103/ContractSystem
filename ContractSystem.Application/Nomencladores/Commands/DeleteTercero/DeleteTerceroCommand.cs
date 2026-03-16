using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.DeleteTercero;

public record DeleteTerceroCommand(int Id) : IRequest<Unit>;

public class DeleteTerceroCommandHandler : IRequestHandler<DeleteTerceroCommand, Unit>
{
    private readonly ITerceroStore _store;

    public DeleteTerceroCommandHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(DeleteTerceroCommand request, CancellationToken cancellationToken)
    {
        await _store.SoftDeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
