using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAnexosByContrato;

public record GetAnexosByContratoQuery(int ContratoId) : IRequest<IReadOnlyList<Anexo>>;

public class GetAnexosByContratoQueryHandler : IRequestHandler<GetAnexosByContratoQuery, IReadOnlyList<Anexo>>
{
    private readonly IAnexoStore _store;

    public GetAnexosByContratoQueryHandler(IAnexoStore store) => _store = store;

    public Task<IReadOnlyList<Anexo>> Handle(GetAnexosByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByContratoAsync(request.ContratoId, cancellationToken);
    }
}
