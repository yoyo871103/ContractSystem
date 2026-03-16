using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.UpdateProductoServicio;

public record UpdateProductoServicioCommand(
    int Id,
    string? Codigo,
    string Nombre,
    string Descripcion,
    TipoProductoServicio Tipo,
    int? UnidadMedidaId,
    decimal? PrecioEstimado) : IRequest<Unit>;

public class UpdateProductoServicioCommandHandler : IRequestHandler<UpdateProductoServicioCommand, Unit>
{
    private readonly IProductoServicioStore _store;

    public UpdateProductoServicioCommandHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(UpdateProductoServicioCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new InvalidOperationException($"Producto/Servicio con Id {request.Id} no encontrado.");

        entity.Codigo = request.Codigo?.Trim();
        entity.Nombre = request.Nombre.Trim();
        entity.Descripcion = request.Descripcion?.Trim() ?? string.Empty;
        entity.Tipo = request.Tipo;
        entity.UnidadMedidaId = request.UnidadMedidaId;
        entity.PrecioEstimado = request.PrecioEstimado;
        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
