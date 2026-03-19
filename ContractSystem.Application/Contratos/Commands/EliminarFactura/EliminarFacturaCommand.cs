using MediatR;

namespace ContractSystem.Application.Contratos.Commands.EliminarFactura;

public record EliminarFacturaCommand(int Id) : IRequest;

public class EliminarFacturaCommandHandler : IRequestHandler<EliminarFacturaCommand>
{
    private readonly IFacturaStore _store;

    public EliminarFacturaCommandHandler(IFacturaStore store) => _store = store;

    public async Task Handle(EliminarFacturaCommand request, CancellationToken cancellationToken)
    {
        await _store.DeleteAsync(request.Id, cancellationToken);
    }
}
