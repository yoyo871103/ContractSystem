using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;

public record GetAllTercerosQuery(bool IncludeDeleted = false, TipoTercero? Tipo = null) : IRequest<IReadOnlyList<Tercero>>;

public class GetAllTercerosQueryHandler : IRequestHandler<GetAllTercerosQuery, IReadOnlyList<Tercero>>
{
    private readonly ITerceroStore _store;

    public GetAllTercerosQueryHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<Tercero>> Handle(GetAllTercerosQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetAllAsync(request.IncludeDeleted, request.Tipo, cancellationToken);
    }
}
