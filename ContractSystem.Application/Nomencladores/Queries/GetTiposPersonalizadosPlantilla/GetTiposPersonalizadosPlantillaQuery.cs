using MediatR;

namespace ContractSystem.Application.Nomencladores.Queries.GetTiposPersonalizadosPlantilla;

/// <summary>
/// Devuelve los nombres de tipos personalizados ("Otros") ya usados en plantillas, para reutilizarlos.
/// </summary>
public record GetTiposPersonalizadosPlantillaQuery : IRequest<IReadOnlyList<string>>;

public class GetTiposPersonalizadosPlantillaQueryHandler : IRequestHandler<GetTiposPersonalizadosPlantillaQuery, IReadOnlyList<string>>
{
    private readonly IPlantillaDocumentoStore _store;

    public GetTiposPersonalizadosPlantillaQueryHandler(IPlantillaDocumentoStore store) => _store = store;

    public Task<IReadOnlyList<string>> Handle(GetTiposPersonalizadosPlantillaQuery request, CancellationToken cancellationToken)
        => _store.GetTiposPersonalizadosAsync(cancellationToken);
}
