using ContractSystem.Application.Common.Models;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetPagedTerceros;

public record GetPagedTercerosQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeDeleted = false,
    TipoTercero? Tipo = null,
    string? SearchText = null) : IRequest<PagedList<Tercero>>;

public class GetPagedTercerosQueryHandler : IRequestHandler<GetPagedTercerosQuery, PagedList<Tercero>>
{
    private readonly ITerceroStore _store;

    public GetPagedTercerosQueryHandler(ITerceroStore store)
    {
        _store = store;
    }

    public Task<PagedList<Tercero>> Handle(GetPagedTercerosQuery request, CancellationToken cancellationToken)
    {
        return _store.GetPagedAsync(request.Page, request.PageSize, request.IncludeDeleted, request.Tipo, request.SearchText, cancellationToken);
    }
}
