using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetAllContratos;

public record GetAllContratosQuery(
    bool IncludeDeleted = false,
    TipoDocumentoContrato? Tipo = null,
    EstadoContrato? Estado = null,
    RolContrato? Rol = null,
    int? TerceroId = null,
    DateTime? FechaFirmaDesde = null,
    DateTime? FechaFirmaHasta = null,
    string? TextoBusqueda = null,
    string? TextoTercero = null) : IRequest<IReadOnlyList<Contrato>>;

public class GetAllContratosQueryHandler : IRequestHandler<GetAllContratosQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetAllContratosQueryHandler(IContratoStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Contrato>> Handle(GetAllContratosQuery request, CancellationToken cancellationToken)
    {
        return _store.GetAllAsync(
            request.IncludeDeleted,
            request.Tipo,
            request.Estado,
            request.Rol,
            request.TerceroId,
            request.FechaFirmaDesde,
            request.FechaFirmaHasta,
            request.TextoBusqueda,
            request.TextoTercero,
            cancellationToken);
    }
}
