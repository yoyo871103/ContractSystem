using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetUnidadMedidaById;

public record GetUnidadMedidaByIdQuery(int Id) : IRequest<UnidadMedida?>;

public class GetUnidadMedidaByIdQueryHandler : IRequestHandler<GetUnidadMedidaByIdQuery, UnidadMedida?>
{
    private readonly IUnidadMedidaStore _store;

    public GetUnidadMedidaByIdQueryHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<UnidadMedida?> Handle(GetUnidadMedidaByIdQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetByIdAsync(request.Id, cancellationToken);
    }
}
