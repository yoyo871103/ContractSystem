using MediatR;

namespace ContractSystem.Application.Contratos.Commands.DeleteAnexo;

public record DeleteAnexoCommand(int Id) : IRequest<Unit>;

public class DeleteAnexoCommandHandler : IRequestHandler<DeleteAnexoCommand, Unit>
{
    private readonly IAnexoStore _store;

    public DeleteAnexoCommandHandler(IAnexoStore store) => _store = store;

    public async Task<Unit> Handle(DeleteAnexoCommand request, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
