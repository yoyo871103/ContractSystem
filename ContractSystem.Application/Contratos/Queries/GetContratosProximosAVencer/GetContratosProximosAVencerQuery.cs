using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetContratosProximosAVencer;

/// <summary>
/// Obtiene contratos vigentes cuya fecha de vigencia está dentro del período de alerta.
/// </summary>
public record GetContratosProximosAVencerQuery(int DiasAntelacion = 30) : IRequest<IReadOnlyList<Contrato>>;

public class GetContratosProximosAVencerQueryHandler
    : IRequestHandler<GetContratosProximosAVencerQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetContratosProximosAVencerQueryHandler(IContratoStore store) => _store = store;

    public async Task<IReadOnlyList<Contrato>> Handle(GetContratosProximosAVencerQuery request, CancellationToken cancellationToken)
    {
        var vigentes = await _store.GetAllAsync(estado: EstadoContrato.Vigente, cancellationToken: cancellationToken);
        var hoy = DateTime.UtcNow.Date;
        var limite = hoy.AddDays(request.DiasAntelacion);

        return vigentes
            .Where(c => c.FechaVigencia.HasValue
                && c.FechaVigencia.Value.Date >= hoy
                && c.FechaVigencia.Value.Date <= limite)
            .OrderBy(c => c.FechaVigencia)
            .ToList();
    }
}
