using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetSuplementosByContrato;

/// <summary>
/// Obtiene los suplementos hijos de un contrato dado.
/// </summary>
public record GetSuplementosByContratoQuery(int ContratoPadreId) : IRequest<IReadOnlyList<Contrato>>;

public class GetSuplementosByContratoQueryHandler : IRequestHandler<GetSuplementosByContratoQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetSuplementosByContratoQueryHandler(IContratoStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Contrato>> Handle(GetSuplementosByContratoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetHijosAsync(request.ContratoPadreId, cancellationToken);
    }
}
