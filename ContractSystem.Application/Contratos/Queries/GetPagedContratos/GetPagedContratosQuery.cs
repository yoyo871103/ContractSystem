using ContractSystem.Application.Common.Models;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetPagedContratos;

public record GetPagedContratosQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeDeleted = false,
    TipoDocumentoContrato? Tipo = null,
    EstadoContrato? Estado = null,
    RolContrato? Rol = null,
    int? TerceroId = null,
    string? TextoBusqueda = null,
    string? TextoTercero = null) : IRequest<PagedList<Contrato>>;

public class GetPagedContratosQueryHandler : IRequestHandler<GetPagedContratosQuery, PagedList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetPagedContratosQueryHandler(IContratoStore store)
    {
        _store = store;
    }

    public Task<PagedList<Contrato>> Handle(GetPagedContratosQuery request, CancellationToken cancellationToken)
    {
        return _store.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.IncludeDeleted,
            request.Tipo,
            request.Estado,
            request.Rol,
            request.TerceroId,
            request.TextoBusqueda,
            request.TextoTercero,
            cancellationToken);
    }
}
