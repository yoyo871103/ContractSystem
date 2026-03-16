using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetHistorialByContrato;

public record GetHistorialByContratoQuery(
    int ContratoId,
    TipoCambio? FiltroTipo = null) : IRequest<IReadOnlyList<HistorialCambio>>;

public class GetHistorialByContratoQueryHandler
    : IRequestHandler<GetHistorialByContratoQuery, IReadOnlyList<HistorialCambio>>
{
    private readonly IHistorialCambioStore _store;

    public GetHistorialByContratoQueryHandler(IHistorialCambioStore store) => _store = store;

    public Task<IReadOnlyList<HistorialCambio>> Handle(GetHistorialByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByContratoAsync(request.ContratoId, request.FiltroTipo, cancellationToken);
    }
}
