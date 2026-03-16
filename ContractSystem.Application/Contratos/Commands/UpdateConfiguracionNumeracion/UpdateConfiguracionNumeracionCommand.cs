using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.UpdateConfiguracionNumeracion;

public record UpdateConfiguracionNumeracionCommand(
    string Formato,
    int DigitosPadding,
    bool ContadorPorAnio) : IRequest<ConfiguracionNumeracion>;

public class UpdateConfiguracionNumeracionCommandHandler : IRequestHandler<UpdateConfiguracionNumeracionCommand, ConfiguracionNumeracion>
{
    private readonly IConfiguracionNumeracionStore _store;

    public UpdateConfiguracionNumeracionCommandHandler(IConfiguracionNumeracionStore store)
    {
        _store = store;
    }

    public async Task<ConfiguracionNumeracion> Handle(UpdateConfiguracionNumeracionCommand request, CancellationToken cancellationToken)
    {
        var existente = await _store.GetActivaAsync(cancellationToken);

        if (existente is not null)
        {
            existente.Formato = request.Formato.Trim();
            existente.DigitosPadding = request.DigitosPadding;
            existente.ContadorPorAnio = request.ContadorPorAnio;
            await _store.UpdateAsync(existente, cancellationToken);
            return existente;
        }

        // Si no existe, crear una nueva
        var nueva = new ConfiguracionNumeracion
        {
            Formato = request.Formato.Trim(),
            DigitosPadding = request.DigitosPadding,
            ContadorPorAnio = request.ContadorPorAnio,
            Activa = true
        };
        return await _store.CreateAsync(nueva, cancellationToken);
    }
}
