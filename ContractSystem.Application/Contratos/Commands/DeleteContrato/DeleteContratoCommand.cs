using MediatR;

namespace ContractSystem.Application.Contratos.Commands.DeleteContrato;

public record DeleteContratoCommand(int Id) : IRequest<Unit>;

public class DeleteContratoCommandHandler : IRequestHandler<DeleteContratoCommand, Unit>
{
    private readonly IContratoStore _store;

    public DeleteContratoCommandHandler(IContratoStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(DeleteContratoCommand request, CancellationToken cancellationToken)
    {
        await _store.SoftDeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
