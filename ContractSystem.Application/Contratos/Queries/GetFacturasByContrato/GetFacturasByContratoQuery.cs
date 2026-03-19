using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetFacturasByContrato;

public record GetFacturasByContratoQuery(int ContratoId) : IRequest<IReadOnlyList<Factura>>;

public class GetFacturasByContratoQueryHandler : IRequestHandler<GetFacturasByContratoQuery, IReadOnlyList<Factura>>
{
    private readonly IFacturaStore _store;

    public GetFacturasByContratoQueryHandler(IFacturaStore store) => _store = store;

    public Task<IReadOnlyList<Factura>> Handle(GetFacturasByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByContratoAsync(request.ContratoId, cancellationToken);
    }
}
