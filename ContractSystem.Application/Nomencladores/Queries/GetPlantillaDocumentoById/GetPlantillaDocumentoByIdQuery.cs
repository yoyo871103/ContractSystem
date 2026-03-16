using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetPlantillaDocumentoById;

public record GetPlantillaDocumentoByIdQuery(int Id) : IRequest<PlantillaDocumento?>;

public class GetPlantillaDocumentoByIdQueryHandler : IRequestHandler<GetPlantillaDocumentoByIdQuery, PlantillaDocumento?>
{
    private readonly IPlantillaDocumentoStore _store;

    public GetPlantillaDocumentoByIdQueryHandler(IPlantillaDocumentoStore store)
    {
        _store = store;
    }

    public async Task<PlantillaDocumento?> Handle(GetPlantillaDocumentoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetByIdAsync(request.Id, cancellationToken);
    }
}
