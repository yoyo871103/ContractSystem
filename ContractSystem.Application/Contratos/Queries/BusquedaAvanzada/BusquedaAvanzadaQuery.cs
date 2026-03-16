using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.BusquedaAvanzada;

/// <summary>
/// Búsqueda avanzada con criterios combinables (Sección 13).
/// </summary>
public record BusquedaAvanzadaQuery(
    int? TerceroId = null,
    string? TextoProducto = null,
    DateTime? FechaFirmaDesde = null,
    DateTime? FechaFirmaHasta = null,
    DateTime? FechaVigenciaDesde = null,
    DateTime? FechaVigenciaHasta = null,
    TipoDocumentoContrato? Tipo = null,
    EstadoContrato? Estado = null,
    string? TextoObjeto = null) : IRequest<IReadOnlyList<Contrato>>;

public class BusquedaAvanzadaQueryHandler : IRequestHandler<BusquedaAvanzadaQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _contratoStore;
    private readonly ILineaDetalleStore _lineaStore;

    public BusquedaAvanzadaQueryHandler(IContratoStore contratoStore, ILineaDetalleStore lineaStore)
    {
        _contratoStore = contratoStore;
        _lineaStore = lineaStore;
    }

    public async Task<IReadOnlyList<Contrato>> Handle(BusquedaAvanzadaQuery request, CancellationToken cancellationToken)
    {
        // Obtener contratos con filtros base
        var todos = await _contratoStore.GetAllAsync(
            tipo: request.Tipo,
            estado: request.Estado,
            terceroId: request.TerceroId,
            fechaFirmaDesde: request.FechaFirmaDesde,
            fechaFirmaHasta: request.FechaFirmaHasta,
            textoBusqueda: request.TextoObjeto,
            cancellationToken: cancellationToken);

        var resultado = todos.ToList();

        // Filtro por rango de vigencia
        if (request.FechaVigenciaDesde.HasValue)
            resultado = resultado.Where(c => c.FechaVigencia >= request.FechaVigenciaDesde).ToList();
        if (request.FechaVigenciaHasta.HasValue)
            resultado = resultado.Where(c => c.FechaVigencia <= request.FechaVigenciaHasta).ToList();

        // Filtro por texto en líneas de detalle (producto/servicio)
        if (!string.IsNullOrWhiteSpace(request.TextoProducto))
        {
            var filtrados = new List<Contrato>();
            foreach (var contrato in resultado)
            {
                var lineas = await _lineaStore.GetByContratoAsync(contrato.Id, cancellationToken);
                if (lineas.Any(l => l.Concepto.Contains(request.TextoProducto, StringComparison.OrdinalIgnoreCase)
                    || (l.Descripcion?.Contains(request.TextoProducto, StringComparison.OrdinalIgnoreCase) == true)))
                {
                    filtrados.Add(contrato);
                }
            }
            resultado = filtrados;
        }

        return resultado;
    }
}
