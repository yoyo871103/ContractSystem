using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.SearchTerceros;

public record SearchTercerosQuery(string Texto, int MaxResults = 20) : IRequest<IReadOnlyList<Tercero>>;

public class SearchTercerosQueryHandler : IRequestHandler<SearchTercerosQuery, IReadOnlyList<Tercero>>
{
    private readonly ITerceroStore _store;

    public SearchTercerosQueryHandler(ITerceroStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Tercero>> Handle(SearchTercerosQuery request, CancellationToken cancellationToken)
    {
        return _store.SearchAsync(request.Texto, request.MaxResults, cancellationToken);
    }
}
