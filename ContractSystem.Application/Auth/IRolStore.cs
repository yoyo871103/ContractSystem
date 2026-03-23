namespace ContractSystem.Application.Auth;

/// <summary>
/// Acceso a roles para gestión y asignación.
/// </summary>
public interface IRolStore
{
    Task<IReadOnlyList<RolItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolDetailDto?> GetByIdAsync(int rolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolListItem>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermisoItem>> GetAllPermisosAsync(CancellationToken cancellationToken = default);
    Task<int> CreateAsync(string nombre, string? descripcion, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default);
    Task UpdateAsync(int rolId, string nombre, string? descripcion, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default);
    Task DeleteAsync(int rolId, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
}
