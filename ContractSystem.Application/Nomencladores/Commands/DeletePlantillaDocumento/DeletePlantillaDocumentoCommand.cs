using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.DeletePlantillaDocumento;

public record DeletePlantillaDocumentoCommand(int Id) : IRequest<Unit>;

public class DeletePlantillaDocumentoCommandHandler : IRequestHandler<DeletePlantillaDocumentoCommand, Unit>
{
    private readonly IPlantillaDocumentoStore _store;

    public DeletePlantillaDocumentoCommandHandler(IPlantillaDocumentoStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(DeletePlantillaDocumentoCommand request, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
