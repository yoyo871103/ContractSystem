using MediatR;

namespace ContractSystem.Application.Contratos.Commands.UpdateAnexo;

public record UpdateAnexoCommand(int Id, string Nombre, string? CondicionesEntrega, string? CostosAsociados, int Orden) : IRequest<Unit>;

public class UpdateAnexoCommandHandler : IRequestHandler<UpdateAnexoCommand, Unit>
{
    private readonly IAnexoStore _store;

    public UpdateAnexoCommandHandler(IAnexoStore store) => _store = store;

    public async Task<Unit> Handle(UpdateAnexoCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Anexo con Id {request.Id} no encontrado.");

        entity.Nombre = request.Nombre.Trim();
        entity.CondicionesEntrega = request.CondicionesEntrega?.Trim();
        entity.CostosAsociados = request.CostosAsociados?.Trim();
        entity.Orden = request.Orden;
        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
