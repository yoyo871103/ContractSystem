using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetContratosMarco;

/// <summary>
/// Obtiene contratos Marco activos (no rescindidos) para seleccionar como padre de un Específico.
/// </summary>
public record GetContratosMarcoQuery() : IRequest<IReadOnlyList<Contrato>>;

public class GetContratosMarcoQueryHandler : IRequestHandler<GetContratosMarcoQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetContratosMarcoQueryHandler(IContratoStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Contrato>> Handle(GetContratosMarcoQuery request, CancellationToken cancellationToken)
    {
        return _store.GetContratosMarcoAsync(cancellationToken);
    }
}
