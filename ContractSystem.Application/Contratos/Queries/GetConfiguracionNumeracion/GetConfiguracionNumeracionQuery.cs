using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetConfiguracionNumeracion;

public record GetConfiguracionNumeracionQuery() : IRequest<ConfiguracionNumeracion?>;

public class GetConfiguracionNumeracionQueryHandler : IRequestHandler<GetConfiguracionNumeracionQuery, ConfiguracionNumeracion?>
{
    private readonly IConfiguracionNumeracionStore _store;

    public GetConfiguracionNumeracionQueryHandler(IConfiguracionNumeracionStore store)
    {
        _store = store;
    }

    public Task<ConfiguracionNumeracion?> Handle(GetConfiguracionNumeracionQuery request, CancellationToken cancellationToken)
    {
        return _store.GetActivaAsync(cancellationToken);
    }
}
