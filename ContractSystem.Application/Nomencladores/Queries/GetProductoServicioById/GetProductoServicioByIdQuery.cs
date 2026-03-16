using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetProductoServicioById;

public record GetProductoServicioByIdQuery(int Id) : IRequest<ProductoServicio?>;

public class GetProductoServicioByIdQueryHandler : IRequestHandler<GetProductoServicioByIdQuery, ProductoServicio?>
{
    private readonly IProductoServicioStore _store;

    public GetProductoServicioByIdQueryHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public async Task<ProductoServicio?> Handle(GetProductoServicioByIdQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetByIdAsync(request.Id, cancellationToken);
    }
}
