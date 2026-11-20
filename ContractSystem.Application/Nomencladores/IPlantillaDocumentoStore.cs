using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

public interface IPlantillaDocumentoStore
{
    Task<PlantillaDocumento?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlantillaDocumento>> GetAllAsync(TipoDocumentoPlantilla? tipo = null, RolPlantilla? rol = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Nombres de tipos personalizados ya usados (TipoDocumento == Otros), para reutilizarlos.
    /// </summary>
    Task<IReadOnlyList<string>> GetTiposPersonalizadosAsync(CancellationToken cancellationToken = default);
    Task<PlantillaDocumento> CreateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
