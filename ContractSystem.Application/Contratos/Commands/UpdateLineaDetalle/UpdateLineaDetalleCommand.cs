using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.UpdateLineaDetalle;

public record UpdateLineaDetalleCommand(
    int Id,
    TipoProductoServicio Tipo,
    string Concepto,
    string? Descripcion,
    decimal Cantidad,
    string? UnidadMedidaTexto,
    int? UnidadMedidaId,
    decimal PrecioUnitario,
    decimal ImporteTotal,
    int Orden) : IRequest<Unit>;

public class UpdateLineaDetalleCommandHandler : IRequestHandler<UpdateLineaDetalleCommand, Unit>
{
    private readonly ILineaDetalleStore _store;

    public UpdateLineaDetalleCommandHandler(ILineaDetalleStore store) => _store = store;

    public async Task<Unit> Handle(UpdateLineaDetalleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Línea de detalle con Id {request.Id} no encontrada.");

        entity.Tipo = request.Tipo;
        entity.Concepto = request.Concepto.Trim();
        entity.Descripcion = request.Descripcion?.Trim();
        entity.Cantidad = request.Cantidad;
        entity.UnidadMedidaTexto = request.UnidadMedidaTexto?.Trim();
        entity.UnidadMedidaId = request.UnidadMedidaId;
        entity.PrecioUnitario = request.PrecioUnitario;
        entity.ImporteTotal = request.Cantidad * request.PrecioUnitario;
        entity.Orden = request.Orden;

        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
