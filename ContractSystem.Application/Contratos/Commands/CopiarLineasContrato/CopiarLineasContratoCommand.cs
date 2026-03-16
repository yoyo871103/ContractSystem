using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CopiarLineasContrato;

/// <summary>
/// Copia los anexos y sus líneas de detalle de un contrato origen a un suplemento destino (flujo 17.2, sección 8.5).
/// </summary>
public record CopiarLineasContratoCommand(int ContratoOrigenId, int SuplementoDestinoId) : IRequest<int>;

public class CopiarLineasContratoCommandHandler : IRequestHandler<CopiarLineasContratoCommand, int>
{
    private readonly IAnexoStore _anexoStore;
    private readonly ILineaDetalleStore _lineaStore;

    public CopiarLineasContratoCommandHandler(IAnexoStore anexoStore, ILineaDetalleStore lineaStore)
    {
        _anexoStore = anexoStore;
        _lineaStore = lineaStore;
    }

    public async Task<int> Handle(CopiarLineasContratoCommand request, CancellationToken cancellationToken)
    {
        // Copiar anexos del contrato origen al destino
        var anexosOrigen = await _anexoStore.GetByContratoAsync(request.ContratoOrigenId, cancellationToken);
        var lineasOrigen = await _lineaStore.GetByContratoAsync(request.ContratoOrigenId, cancellationToken);

        // Mapeo de AnexoId original → AnexoId nuevo
        var anexoMap = new Dictionary<int, int>();
        foreach (var anexo in anexosOrigen)
        {
            var nuevoAnexo = new Domain.Contratos.Anexo
            {
                ContratoId = request.SuplementoDestinoId,
                Nombre = anexo.Nombre,
                CondicionesEntrega = anexo.CondicionesEntrega,
                CostosAsociados = anexo.CostosAsociados,
                Orden = anexo.Orden
            };
            var creado = await _anexoStore.CreateAsync(nuevoAnexo, cancellationToken);
            anexoMap[anexo.Id] = creado.Id;
        }

        var count = 0;
        foreach (var linea in lineasOrigen)
        {
            if (!anexoMap.TryGetValue(linea.AnexoId, out var nuevoAnexoId))
                continue; // Skip orphan lines

            var copia = new Domain.Contratos.LineaDetalle
            {
                ContratoId = request.SuplementoDestinoId,
                AnexoId = nuevoAnexoId,
                Tipo = linea.Tipo,
                Concepto = linea.Concepto,
                Descripcion = linea.Descripcion,
                Cantidad = linea.Cantidad,
                UnidadMedidaTexto = linea.UnidadMedidaTexto,
                UnidadMedidaId = linea.UnidadMedidaId,
                PrecioUnitario = linea.PrecioUnitario,
                ImporteTotal = linea.ImporteTotal,
                ProductoServicioOrigenId = linea.ProductoServicioOrigenId,
                EsCopiaDeOriginal = true,
                Orden = linea.Orden
            };
            await _lineaStore.CreateAsync(copia, cancellationToken);
            count++;
        }

        return count;
    }
}
