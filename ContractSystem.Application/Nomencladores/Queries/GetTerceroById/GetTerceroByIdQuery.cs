using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetTerceroById;

public record GetTerceroByIdQuery(int Id, bool IncludeContactos = true) : IRequest<Tercero?>;

public class GetTerceroByIdQueryHandler : IRequestHandler<GetTerceroByIdQuery, Tercero?>
{
    private readonly ITerceroStore _store;

    public GetTerceroByIdQueryHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<Tercero?> Handle(GetTerceroByIdQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetByIdAsync(request.Id, request.IncludeContactos, cancellationToken);
    }
}
