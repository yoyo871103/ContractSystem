using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetAllPlantillasDocumento;

public record GetAllPlantillasDocumentoQuery(TipoDocumentoPlantilla? Tipo = null, RolPlantilla? Rol = null) : IRequest<IReadOnlyList<PlantillaDocumento>>;

public class GetAllPlantillasDocumentoQueryHandler : IRequestHandler<GetAllPlantillasDocumentoQuery, IReadOnlyList<PlantillaDocumento>>
{
    private readonly IPlantillaDocumentoStore _store;

    public GetAllPlantillasDocumentoQueryHandler(IPlantillaDocumentoStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<PlantillaDocumento>> Handle(GetAllPlantillasDocumentoQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetAllAsync(request.Tipo, request.Rol, cancellationToken);
    }
}
