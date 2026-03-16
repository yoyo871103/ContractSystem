using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetResumenContratos;

public record ResumenContratos(
    int TotalContratos,
    int Borradores,
    int Vigentes,
    int Vencidos,
    int Rescindidos,
    int Ejecutados,
    int ProximosAVencer);

public record GetResumenContratosQuery(int DiasAlerta = 30) : IRequest<ResumenContratos>;

public class GetResumenContratosQueryHandler : IRequestHandler<GetResumenContratosQuery, ResumenContratos>
{
    private readonly IContratoStore _store;

    public GetResumenContratosQueryHandler(IContratoStore store) => _store = store;

    public async Task<ResumenContratos> Handle(GetResumenContratosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _store.GetAllAsync(cancellationToken: cancellationToken);
        var hoy = DateTime.UtcNow.Date;
        var limite = hoy.AddDays(request.DiasAlerta);

        var proximosAVencer = todos.Count(c =>
            c.Estado == EstadoContrato.Vigente
            && c.FechaVigencia.HasValue
            && c.FechaVigencia.Value.Date >= hoy
            && c.FechaVigencia.Value.Date <= limite);

        return new ResumenContratos(
            TotalContratos: todos.Count,
            Borradores: todos.Count(c => c.Estado == EstadoContrato.Borrador),
            Vigentes: todos.Count(c => c.Estado == EstadoContrato.Vigente),
            Vencidos: todos.Count(c => c.Estado == EstadoContrato.Vencido),
            Rescindidos: todos.Count(c => c.Estado == EstadoContrato.Rescindido),
            Ejecutados: todos.Count(c => c.Estado == EstadoContrato.Ejecutado),
            ProximosAVencer: proximosAVencer);
    }
}
