using ContractSystem.Application.Common.Models;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetPagedProductosServicios;

public record GetPagedProductosServiciosQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeDeleted = false,
    TipoProductoServicio? Tipo = null,
    string? SearchText = null) : IRequest<PagedList<ProductoServicio>>;

public class GetPagedProductosServiciosQueryHandler : IRequestHandler<GetPagedProductosServiciosQuery, PagedList<ProductoServicio>>
{
    private readonly IProductoServicioStore _store;

    public GetPagedProductosServiciosQueryHandler(IProductoServicioStore store)
    {
        _store = store;
    }

    public Task<PagedList<ProductoServicio>> Handle(GetPagedProductosServiciosQuery request, CancellationToken cancellationToken)
    {
        return _store.GetPagedAsync(request.Page, request.PageSize, request.IncludeDeleted, request.Tipo, request.SearchText, cancellationToken);
    }
}
