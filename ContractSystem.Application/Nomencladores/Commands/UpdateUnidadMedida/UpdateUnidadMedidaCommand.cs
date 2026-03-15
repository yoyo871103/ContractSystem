using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.UpdateUnidadMedida;

public record UpdateUnidadMedidaCommand(int Id, string NombreCorto, string Descripcion) : IRequest<Unit>;

public class UpdateUnidadMedidaCommandHandler : IRequestHandler<UpdateUnidadMedidaCommand, Unit>
{
    private readonly IUnidadMedidaStore _store;

    public UpdateUnidadMedidaCommandHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(UpdateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new InvalidOperationException($"Unidad de medida con Id {request.Id} no encontrada.");

        entity.NombreCorto = request.NombreCorto.Trim();
        entity.Descripcion = request.Descripcion?.Trim() ?? string.Empty;
        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
