using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetInformesContratos;

// --- DTOs de informes ---

/// <summary>Facturación agrupada por contrato/suplemento.</summary>
public record FacturacionPorContratoDto(
    int ContratoId,
    string Numero,
    string Objeto,
    string TipoDocumento,
    string? Tercero,
    decimal ValorContrato,
    int CantidadFacturas,
    decimal TotalFacturado,
    decimal Diferencia);

/// <summary>Contratos agrupados por tercero.</summary>
public record ContratosPorTerceroDto(
    string Tercero,
    int TotalContratos,
    int Vigentes,
    int Vencidos,
    decimal ValorTotal,
    decimal TotalFacturado);

/// <summary>Contratos agrupados por tipo de documento.</summary>
public record ContratosPorTipoDto(
    string Tipo,
    int Cantidad,
    decimal ValorTotal,
    decimal TotalFacturado);

/// <summary>Contratos agrupados por estado.</summary>
public record ContratosPorEstadoDto(
    string Estado,
    int Cantidad,
    decimal ValorTotal);

/// <summary>Contratos próximos a vencer.</summary>
public record ContratoProximoVencerDto(
    string Numero,
    string Objeto,
    string? Tercero,
    string TipoDocumento,
    DateTime FechaVigencia,
    int DiasRestantes);

/// <summary>Contratos por rol (Proveedor/Cliente).</summary>
public record ContratosPorRolDto(
    string Rol,
    int Cantidad,
    decimal ValorTotal,
    decimal TotalFacturado);

/// <summary>Resultado completo de informes.</summary>
public record InformesContratosResult(
    IReadOnlyList<FacturacionPorContratoDto> FacturacionPorContrato,
    IReadOnlyList<ContratosPorTerceroDto> ContratosPorTercero,
    IReadOnlyList<ContratosPorTipoDto> ContratosPorTipo,
    IReadOnlyList<ContratosPorEstadoDto> ContratosPorEstado,
    IReadOnlyList<ContratoProximoVencerDto> ProximosAVencer,
    IReadOnlyList<ContratosPorRolDto> ContratosPorRol);

// --- Query ---

public record GetInformesContratosQuery(
    int DiasAlertaVencimiento = 60,
    string? TextoTercero = null,
    string? TextoNumero = null,
    string? TextoProductoServicio = null) : IRequest<InformesContratosResult>;

public class GetInformesContratosQueryHandler : IRequestHandler<GetInformesContratosQuery, InformesContratosResult>
{
    private readonly IContratoStore _contratoStore;
    private readonly IFacturaStore _facturaStore;

    public GetInformesContratosQueryHandler(IContratoStore contratoStore, IFacturaStore facturaStore)
    {
        _contratoStore = contratoStore;
        _facturaStore = facturaStore;
    }

    public async Task<InformesContratosResult> Handle(GetInformesContratosQuery request, CancellationToken cancellationToken)
    {
        var contratos = await _contratoStore.GetAllAsync(
            textoBusqueda: request.TextoNumero,
            textoTercero: request.TextoTercero,
            textoProductoServicio: request.TextoProductoServicio,
            cancellationToken: cancellationToken);
        var hoy = DateTime.UtcNow.Date;

        // Cargar facturas para todos los contratos no-Marco
        var contratosConFacturas = contratos
            .Where(c => c.TipoDocumento != TipoDocumentoContrato.Marco)
            .ToList();

        var facturasDict = new Dictionary<int, IReadOnlyList<Factura>>();
        foreach (var c in contratosConFacturas)
        {
            var facturas = await _facturaStore.GetByContratoAsync(c.Id, cancellationToken);
            facturasDict[c.Id] = facturas;
        }

        // 1. Facturación por contrato/suplemento
        var facturacionPorContrato = contratosConFacturas
            .Select(c =>
            {
                var facturas = facturasDict.GetValueOrDefault(c.Id, Array.Empty<Factura>());
                var totalFacturado = facturas.Sum(f => f.ImporteTotal);
                var valorContrato = c.ValorTotal ?? 0;
                return new FacturacionPorContratoDto(
                    c.Id, c.Numero, c.Objeto,
                    c.TipoDocumento.ToString(),
                    c.Tercero?.Nombre,
                    valorContrato,
                    facturas.Count,
                    totalFacturado,
                    valorContrato - totalFacturado);
            })
            .Where(f => f.CantidadFacturas > 0 || f.ValorContrato > 0)
            .OrderByDescending(f => f.TotalFacturado)
            .ToList();

        // 2. Contratos por tercero
        var contratosPorTercero = contratos
            .Where(c => c.Tercero is not null)
            .GroupBy(c => c.Tercero!.Nombre)
            .Select(g =>
            {
                var ids = g.Select(c => c.Id).ToHashSet();
                var totalFacturado = facturasDict
                    .Where(kv => ids.Contains(kv.Key))
                    .SelectMany(kv => kv.Value)
                    .Sum(f => f.ImporteTotal);

                return new ContratosPorTerceroDto(
                    g.Key,
                    g.Count(),
                    g.Count(c => c.Estado == EstadoContrato.Vigente),
                    g.Count(c => c.Estado == EstadoContrato.Vencido),
                    g.Sum(c => c.ValorTotal ?? 0),
                    totalFacturado);
            })
            .OrderByDescending(x => x.TotalContratos)
            .ToList();

        // 3. Contratos por tipo
        var contratosPorTipo = contratos
            .GroupBy(c => c.TipoDocumento)
            .Select(g =>
            {
                var ids = g.Select(c => c.Id).ToHashSet();
                var totalFacturado = facturasDict
                    .Where(kv => ids.Contains(kv.Key))
                    .SelectMany(kv => kv.Value)
                    .Sum(f => f.ImporteTotal);

                return new ContratosPorTipoDto(
                    g.Key.ToString(),
                    g.Count(),
                    g.Sum(c => c.ValorTotal ?? 0),
                    totalFacturado);
            })
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        // 4. Contratos por estado
        var contratosPorEstado = contratos
            .GroupBy(c => c.Estado)
            .Select(g => new ContratosPorEstadoDto(
                g.Key.ToString(),
                g.Count(),
                g.Sum(c => c.ValorTotal ?? 0)))
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        // 5. Próximos a vencer
        var limite = hoy.AddDays(request.DiasAlertaVencimiento);
        var proximosAVencer = contratos
            .Where(c => c.Estado == EstadoContrato.Vigente
                        && c.FechaVigencia.HasValue
                        && c.FechaVigencia.Value.Date >= hoy
                        && c.FechaVigencia.Value.Date <= limite)
            .Select(c => new ContratoProximoVencerDto(
                c.Numero, c.Objeto,
                c.Tercero?.Nombre,
                c.TipoDocumento.ToString(),
                c.FechaVigencia!.Value,
                (int)(c.FechaVigencia.Value.Date - hoy).TotalDays))
            .OrderBy(x => x.DiasRestantes)
            .ToList();

        // 6. Contratos por rol
        var contratosPorRol = contratos
            .GroupBy(c => c.Rol)
            .Select(g =>
            {
                var ids = g.Select(c => c.Id).ToHashSet();
                var totalFacturado = facturasDict
                    .Where(kv => ids.Contains(kv.Key))
                    .SelectMany(kv => kv.Value)
                    .Sum(f => f.ImporteTotal);

                return new ContratosPorRolDto(
                    g.Key.ToString(),
                    g.Count(),
                    g.Sum(c => c.ValorTotal ?? 0),
                    totalFacturado);
            })
            .ToList();

        return new InformesContratosResult(
            facturacionPorContrato,
            contratosPorTercero,
            contratosPorTipo,
            contratosPorEstado,
            proximosAVencer,
            contratosPorRol);
    }
}
