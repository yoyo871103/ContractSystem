using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

public interface IPlantillaDocumentoStore
{
    Task<PlantillaDocumento?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlantillaDocumento>> GetAllAsync(TipoDocumentoPlantilla? tipo = null, RolPlantilla? rol = null, CancellationToken cancellationToken = default);
    Task<PlantillaDocumento> CreateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
