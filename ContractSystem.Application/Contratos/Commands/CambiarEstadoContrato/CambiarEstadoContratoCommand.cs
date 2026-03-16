using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CambiarEstadoContrato;

/// <summary>
/// Cambia el estado de un contrato/suplemento. Los estados son revertibles (R07)
/// pero siempre se registra en el historial.
/// </summary>
public record CambiarEstadoContratoCommand(
    int ContratoId,
    EstadoContrato NuevoEstado) : IRequest;

public class CambiarEstadoContratoCommandHandler : IRequestHandler<CambiarEstadoContratoCommand>
{
    private readonly IContratoStore _contratoStore;
    private readonly IHistorialCambioStore _historialStore;

    public CambiarEstadoContratoCommandHandler(IContratoStore contratoStore, IHistorialCambioStore historialStore)
    {
        _contratoStore = contratoStore;
        _historialStore = historialStore;
    }

    public async Task Handle(CambiarEstadoContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _contratoStore.GetByIdAsync(request.ContratoId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Contrato no encontrado.");

        var estadoAnterior = contrato.Estado;
        if (estadoAnterior == request.NuevoEstado)
            return;

        contrato.Estado = request.NuevoEstado;
        await _contratoStore.UpdateAsync(contrato, cancellationToken);

        await _historialStore.CreateAsync(new HistorialCambio
        {
            ContratoId = request.ContratoId,
            TipoCambio = TipoCambio.Estado,
            Descripcion = $"Estado cambió de {estadoAnterior} a {request.NuevoEstado}",
            ValorAnterior = $"\"{estadoAnterior}\"",
            ValorNuevo = $"\"{request.NuevoEstado}\""
        }, cancellationToken);
    }
}
