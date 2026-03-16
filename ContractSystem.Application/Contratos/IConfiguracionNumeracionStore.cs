using ContractSystem.Domain.Contratos;

namespace ContractSystem.Application.Contratos;

public interface IConfiguracionNumeracionStore
{
    Task<ConfiguracionNumeracion?> GetActivaAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracionNumeracion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConfiguracionNumeracion> CreateAsync(ConfiguracionNumeracion configuracion, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConfiguracionNumeracion configuracion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el siguiente número secuencial, incrementando el contador atómicamente.
    /// </summary>
    Task<int> ObtenerSiguienteNumeroAsync(int? anio, CancellationToken cancellationToken = default);
}
