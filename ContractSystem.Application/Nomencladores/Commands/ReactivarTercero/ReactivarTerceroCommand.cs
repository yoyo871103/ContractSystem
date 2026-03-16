using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.ReactivarTercero;

public record ReactivarTerceroCommand(int Id) : IRequest<Unit>;

public class ReactivarTerceroCommandHandler : IRequestHandler<ReactivarTerceroCommand, Unit>
{
    private readonly ITerceroStore _store;

    public ReactivarTerceroCommandHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(ReactivarTerceroCommand request, CancellationToken cancellationToken)
    {
        await _store.UndeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
