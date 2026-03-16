using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CreateLineaDetalle;

public record CreateLineaDetalleCommand(
    int ContratoId,
    int AnexoId,
    TipoProductoServicio Tipo,
    string Concepto,
    string? Descripcion,
    decimal Cantidad,
    string? UnidadMedidaTexto,
    int? UnidadMedidaId,
    decimal PrecioUnitario,
    int? ProductoServicioOrigenId,
    int Orden) : IRequest<LineaDetalle>;

public class CreateLineaDetalleCommandHandler : IRequestHandler<CreateLineaDetalleCommand, LineaDetalle>
{
    private readonly ILineaDetalleStore _store;

    public CreateLineaDetalleCommandHandler(ILineaDetalleStore store) => _store = store;

    public Task<LineaDetalle> Handle(CreateLineaDetalleCommand request, CancellationToken cancellationToken)
    {
        var linea = new LineaDetalle
        {
            ContratoId = request.ContratoId,
            AnexoId = request.AnexoId,
            Tipo = request.Tipo,
            Concepto = request.Concepto.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Cantidad = request.Cantidad,
            UnidadMedidaTexto = request.UnidadMedidaTexto?.Trim(),
            UnidadMedidaId = request.UnidadMedidaId,
            PrecioUnitario = request.PrecioUnitario,
            ImporteTotal = request.Cantidad * request.PrecioUnitario,
            ProductoServicioOrigenId = request.ProductoServicioOrigenId,
            Orden = request.Orden
        };
        return _store.CreateAsync(linea, cancellationToken);
    }
}
