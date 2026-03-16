using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.CreateProductoServicio;

public record CreateProductoServicioCommand(
    string? Codigo,
    string Nombre,
    string Descripcion,
    TipoProductoServicio Tipo,
    int? UnidadMedidaId,
    decimal? PrecioEstimado) : IRequest<ProductoServicio>;

public class CreateProductoServicioCommandHandler : IRequestHandler<CreateProductoServicioCommand, ProductoServicio>
{
    private readonly IProductoServicioStore _store;

    public CreateProductoServicioCommandHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<ProductoServicio> Handle(CreateProductoServicioCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProductoServicio
        {
            Codigo = request.Codigo?.Trim(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim() ?? string.Empty,
            Tipo = request.Tipo,
            UnidadMedidaId = request.UnidadMedidaId,
            PrecioEstimado = request.PrecioEstimado
        };
        return await _store.CreateAsync(entity, cancellationToken);
    }
}
