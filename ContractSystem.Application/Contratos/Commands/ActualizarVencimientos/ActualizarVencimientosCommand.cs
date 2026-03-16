using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.ActualizarVencimientos;

/// <summary>
/// Detecta contratos con fecha de vigencia pasada y marca como VENCIDO automáticamente.
/// Se ejecuta al iniciar la aplicación.
/// </summary>
public record ActualizarVencimientosCommand : IRequest<int>;

public class ActualizarVencimientosCommandHandler : IRequestHandler<ActualizarVencimientosCommand, int>
{
    private readonly IContratoStore _contratoStore;
    private readonly IHistorialCambioStore _historialStore;

    public ActualizarVencimientosCommandHandler(IContratoStore contratoStore, IHistorialCambioStore historialStore)
    {
        _contratoStore = contratoStore;
        _historialStore = historialStore;
    }

    public async Task<int> Handle(ActualizarVencimientosCommand request, CancellationToken cancellationToken)
    {
        // Obtener contratos vigentes
        var vigentes = await _contratoStore.GetAllAsync(
            estado: EstadoContrato.Vigente,
            cancellationToken: cancellationToken);

        var hoy = DateTime.UtcNow.Date;
        var count = 0;

        foreach (var contrato in vigentes)
        {
            if (contrato.FechaVigencia.HasValue && contrato.FechaVigencia.Value.Date < hoy)
            {
                contrato.Estado = EstadoContrato.Vencido;
                await _contratoStore.UpdateAsync(contrato, cancellationToken);

                await _historialStore.CreateAsync(new HistorialCambio
                {
                    ContratoId = contrato.Id,
                    TipoCambio = TipoCambio.Estado,
                    Descripcion = $"Estado cambió de Vigente a Vencido (vencimiento automático, fecha vigencia: {contrato.FechaVigencia:dd/MM/yyyy})",
                    ValorAnterior = $"\"{EstadoContrato.Vigente}\"",
                    ValorNuevo = $"\"{EstadoContrato.Vencido}\""
                }, cancellationToken);

                count++;
            }
        }

        return count;
    }
}
