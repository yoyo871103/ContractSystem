using ContractSystem.Application.Contratos.Queries.GetAdjuntosByContratos;
using ContractSystem.Application.Contratos.Queries.GetAnexosByContrato;
using ContractSystem.Application.Contratos.Queries.GetFacturasByContrato;
using ContractSystem.Application.Contratos.Queries.GetSuplementosByContrato;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Reúne todos los documentos de un contrato a los que se pueden asociar adjuntos
/// (el propio contrato, sus anexos, sus facturas y sus suplementos con sus anexos y facturas)
/// y todos los adjuntos existentes, etiquetados con el documento al que pertenecen.
/// </summary>
public sealed class AdjuntosContrato
{
    public IReadOnlyList<AdjuntoDestino> Destinos { get; private init; } = Array.Empty<AdjuntoDestino>();
    public IReadOnlyList<AdjuntoListItem> Adjuntos { get; private init; } = Array.Empty<AdjuntoListItem>();

    public static async Task<AdjuntosContrato> CargarAsync(
        ISender sender, int contratoId, string contratoNumero, TipoDocumentoContrato tipo,
        CancellationToken ct = default)
    {
        var destinos = new List<AdjuntoDestino>();

        var principal = new AdjuntoDestino(
            tipo == TipoDocumentoContrato.Suplemento ? TipoAsociacionAdjunto.Suplemento : TipoAsociacionAdjunto.Contrato,
            $"{NombreTipo(tipo)} {contratoNumero}", contratoId);
        destinos.Add(principal);
        await AgregarSubdocumentosAsync(sender, contratoId, tipo, sufijo: "", destinos, ct);

        // Suplementos del contrato (y sus anexos/facturas).
        if (tipo != TipoDocumentoContrato.Suplemento)
        {
            var hijos = await sender.Send(new GetSuplementosByContratoQuery(contratoId), ct);
            foreach (var s in hijos.Where(h => h.TipoDocumento == TipoDocumentoContrato.Suplemento && !h.IsDeleted)
                                   .OrderBy(h => h.Numero))
            {
                destinos.Add(new AdjuntoDestino(TipoAsociacionAdjunto.Suplemento, $"Suplemento {s.Numero}", s.Id));
                await AgregarSubdocumentosAsync(sender, s.Id, s.TipoDocumento, sufijo: $" · Supl. {s.Numero}", destinos, ct);
            }
        }

        var contratoIds = destinos.Select(d => d.ContratoId).Distinct().ToList();
        var adjuntos = await sender.Send(new GetAdjuntosByContratosQuery(contratoIds), ct);

        var items = adjuntos
            .Select(a =>
            {
                var destino = destinos.FirstOrDefault(d => d.Corresponde(a));
                return destino is null
                    ? new AdjuntoListItem(a, "(documento no disponible)")
                    : new AdjuntoListItem(a, destino.Descripcion, destino.Tipo);
            })
            .OrderBy(i => i.TipoAsociacion)
            .ThenBy(i => i.AsociadoA)
            .ThenBy(i => i.FechaCarga)
            .ToList();

        return new AdjuntosContrato { Destinos = destinos, Adjuntos = items };
    }

    private static async Task AgregarSubdocumentosAsync(
        ISender sender, int contratoId, TipoDocumentoContrato tipo, string sufijo,
        List<AdjuntoDestino> destinos, CancellationToken ct)
    {
        var anexos = await sender.Send(new GetAnexosByContratoQuery(contratoId), ct);
        foreach (var a in anexos.OrderBy(a => a.Orden))
            destinos.Add(new AdjuntoDestino(TipoAsociacionAdjunto.Anexo, $"Anexo «{a.Nombre}»{sufijo}", contratoId, AnexoId: a.Id));

        // Los Contratos Marco no admiten facturas.
        if (tipo == TipoDocumentoContrato.Marco) return;

        var facturas = await sender.Send(new GetFacturasByContratoQuery(contratoId), ct);
        foreach (var f in facturas.OrderBy(f => f.Fecha))
            destinos.Add(new AdjuntoDestino(TipoAsociacionAdjunto.Factura, $"Factura {f.Numero}{sufijo}", contratoId, FacturaId: f.Id));
    }

    public static string NombreTipo(TipoDocumentoContrato tipo) => tipo switch
    {
        TipoDocumentoContrato.Marco => "Contrato Marco",
        TipoDocumentoContrato.Especifico => "Contrato Específico",
        TipoDocumentoContrato.Independiente => "Contrato",
        TipoDocumentoContrato.Suplemento => "Suplemento",
        _ => "Contrato"
    };
}
