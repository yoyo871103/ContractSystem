using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.RescindirContrato;

/// <summary>
/// Rescinde un contrato y aplica rescisión en cascada (R01, R02):
/// - Marco → rescinde Específicos → rescinde Suplementos
/// - Específico/Independiente → rescinde Suplementos
/// </summary>
public record RescindirContratoCommand(int ContratoId) : IRequest<IReadOnlyList<Contrato>>;

public class RescindirContratoCommandHandler : IRequestHandler<RescindirContratoCommand, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _contratoStore;
    private readonly IHistorialCambioStore _historialStore;

    public RescindirContratoCommandHandler(IContratoStore contratoStore, IHistorialCambioStore historialStore)
    {
        _contratoStore = contratoStore;
        _historialStore = historialStore;
    }

    public async Task<IReadOnlyList<Contrato>> Handle(RescindirContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _contratoStore.GetByIdAsync(request.ContratoId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Contrato no encontrado.");

        var rescindidos = new List<Contrato>();
        await RescindirRecursivoAsync(contrato, rescindidos, cancellationToken);
        return rescindidos;
    }

    private async Task RescindirRecursivoAsync(Contrato contrato, List<Contrato> rescindidos, CancellationToken cancellationToken)
    {
        var estadoAnterior = contrato.Estado;
        if (estadoAnterior == EstadoContrato.Rescindido)
            return;

        contrato.Estado = EstadoContrato.Rescindido;
        await _contratoStore.UpdateAsync(contrato, cancellationToken);
        rescindidos.Add(contrato);

        await _historialStore.CreateAsync(new HistorialCambio
        {
            ContratoId = contrato.Id,
            TipoCambio = TipoCambio.Estado,
            Descripcion = $"Estado cambió de {estadoAnterior} a Rescindido (cascada)",
            ValorAnterior = $"\"{estadoAnterior}\"",
            ValorNuevo = $"\"{EstadoContrato.Rescindido}\""
        }, cancellationToken);

        // Cascada: rescindir hijos
        var hijos = await _contratoStore.GetHijosAsync(contrato.Id, cancellationToken);
        foreach (var hijo in hijos)
        {
            await RescindirRecursivoAsync(hijo, rescindidos, cancellationToken);
        }
    }
}
