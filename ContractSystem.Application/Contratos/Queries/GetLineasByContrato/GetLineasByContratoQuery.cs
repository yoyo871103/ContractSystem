using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetLineasByContrato;

public record GetLineasByContratoQuery(int ContratoId) : IRequest<IReadOnlyList<LineaDetalle>>;

public class GetLineasByContratoQueryHandler : IRequestHandler<GetLineasByContratoQuery, IReadOnlyList<LineaDetalle>>
{
    private readonly ILineaDetalleStore _store;

    public GetLineasByContratoQueryHandler(ILineaDetalleStore store) => _store = store;

    public Task<IReadOnlyList<LineaDetalle>> Handle(GetLineasByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByContratoAsync(request.ContratoId, cancellationToken);
    }
}
