using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetAllUnidadesMedida;

public record GetAllUnidadesMedidaQuery(bool IncludeDeleted = false) : IRequest<IReadOnlyList<UnidadMedida>>;

public class GetAllUnidadesMedidaQueryHandler : IRequestHandler<GetAllUnidadesMedidaQuery, IReadOnlyList<UnidadMedida>>
{
    private readonly IUnidadMedidaStore _store;

    public GetAllUnidadesMedidaQueryHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<UnidadMedida>> Handle(GetAllUnidadesMedidaQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetAllAsync(request.IncludeDeleted, cancellationToken);
    }
}
