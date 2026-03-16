using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.EjecutarContrato;

/// <summary>
/// Marca un contrato como Ejecutado y opcionalmente sus suplementos seleccionados (R08).
/// </summary>
public record EjecutarContratoCommand(
    int ContratoId,
    IReadOnlyList<int> SuplementosAEjecutar) : IRequest;

public class EjecutarContratoCommandHandler : IRequestHandler<EjecutarContratoCommand>
{
    private readonly IContratoStore _contratoStore;
    private readonly IHistorialCambioStore _historialStore;

    public EjecutarContratoCommandHandler(IContratoStore contratoStore, IHistorialCambioStore historialStore)
    {
        _contratoStore = contratoStore;
        _historialStore = historialStore;
    }

    public async Task Handle(EjecutarContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _contratoStore.GetByIdAsync(request.ContratoId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Contrato no encontrado.");

        // Marcar contrato principal
        await MarcarComoEjecutadoAsync(contrato, cancellationToken);

        // Marcar suplementos seleccionados
        foreach (var suplementoId in request.SuplementosAEjecutar)
        {
            var suplemento = await _contratoStore.GetByIdAsync(suplementoId, cancellationToken: cancellationToken);
            if (suplemento is not null && suplemento.Estado != EstadoContrato.Ejecutado)
            {
                await MarcarComoEjecutadoAsync(suplemento, cancellationToken);
            }
        }
    }

    private async Task MarcarComoEjecutadoAsync(Contrato contrato, CancellationToken cancellationToken)
    {
        var estadoAnterior = contrato.Estado;
        contrato.Estado = EstadoContrato.Ejecutado;
        contrato.Ejecutado = true;
        contrato.FechaEjecucion = DateTime.UtcNow;
        await _contratoStore.UpdateAsync(contrato, cancellationToken);

        await _historialStore.CreateAsync(new HistorialCambio
        {
            ContratoId = contrato.Id,
            TipoCambio = TipoCambio.Estado,
            Descripcion = $"Estado cambió de {estadoAnterior} a Ejecutado",
            ValorAnterior = $"\"{estadoAnterior}\"",
            ValorNuevo = $"\"{EstadoContrato.Ejecutado}\""
        }, cancellationToken);
    }
}
