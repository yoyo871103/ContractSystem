using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetContratoById;

public record GetContratoByIdQuery(int Id, bool IncludeRelaciones = false) : IRequest<Contrato?>;

public class GetContratoByIdQueryHandler : IRequestHandler<GetContratoByIdQuery, Contrato?>
{
    private readonly IContratoStore _store;

    public GetContratoByIdQueryHandler(IContratoStore store)
    {
        _store = store;
    }

    public Task<Contrato?> Handle(GetContratoByIdQuery request, CancellationToken cancellationToken)
    {
        return _store.GetByIdAsync(request.Id, request.IncludeRelaciones, cancellationToken);
    }
}
