using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetAllProductosServicios;

public record GetAllProductosServiciosQuery(bool IncludeDeleted = false, TipoProductoServicio? Tipo = null) : IRequest<IReadOnlyList<ProductoServicio>>;

public class GetAllProductosServiciosQueryHandler : IRequestHandler<GetAllProductosServiciosQuery, IReadOnlyList<ProductoServicio>>
{
    private readonly IProductoServicioStore _store;

    public GetAllProductosServiciosQueryHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<ProductoServicio>> Handle(GetAllProductosServiciosQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetAllAsync(request.IncludeDeleted, request.Tipo, cancellationToken);
    }
}
