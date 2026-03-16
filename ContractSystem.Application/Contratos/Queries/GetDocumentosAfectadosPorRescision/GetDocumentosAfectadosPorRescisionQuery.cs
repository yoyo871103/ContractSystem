using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetDocumentosAfectadosPorRescision;

/// <summary>
/// Obtiene la lista de documentos que serían rescindidos en cascada si se rescinde el contrato indicado.
/// Sirve para mostrar al usuario antes de confirmar.
/// </summary>
public record GetDocumentosAfectadosPorRescisionQuery(int ContratoId) : IRequest<IReadOnlyList<Contrato>>;

public class GetDocumentosAfectadosPorRescisionQueryHandler
    : IRequestHandler<GetDocumentosAfectadosPorRescisionQuery, IReadOnlyList<Contrato>>
{
    private readonly IContratoStore _store;

    public GetDocumentosAfectadosPorRescisionQueryHandler(IContratoStore store) => _store = store;

    public async Task<IReadOnlyList<Contrato>> Handle(GetDocumentosAfectadosPorRescisionQuery request, CancellationToken cancellationToken)
    {
        var contrato = await _store.GetByIdAsync(request.ContratoId, cancellationToken: cancellationToken);
        if (contrato is null) return Array.Empty<Contrato>();

        var afectados = new List<Contrato>();
        await RecopilarAfectadosAsync(contrato, afectados, cancellationToken);
        return afectados;
    }

    private async Task RecopilarAfectadosAsync(Contrato contrato, List<Contrato> afectados, CancellationToken cancellationToken)
    {
        var hijos = await _store.GetHijosAsync(contrato.Id, cancellationToken);
        foreach (var hijo in hijos)
        {
            if (hijo.Estado == EstadoContrato.Rescindido) continue;
            afectados.Add(hijo);
            await RecopilarAfectadosAsync(hijo, afectados, cancellationToken);
        }
    }
}
