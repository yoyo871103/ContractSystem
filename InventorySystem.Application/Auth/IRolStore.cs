namespace InventorySystem.Application.Auth;

/// <summary>
/// Acceso a roles para asignación en gestión de usuarios.
/// </summary>
public interface IRolStore
{
    /// <summary>
    /// Obtiene todos los roles del sistema.
    /// </summary>
    Task<IReadOnlyList<RolItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
