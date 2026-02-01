namespace InventorySystem.Application.Auth;

/// <summary>
/// Asegura que existan datos iniciales (roles y usuario administrador) en la BD.
/// Se ejecuta al arranque cuando hay conexión configurada.
/// </summary>
public interface ISeedDataService
{
    /// <summary>
    /// Crea el rol Administrador y el usuario "admin" si no existen.
    /// El usuario admin tendrá RequiereCambioContraseña = true con una contraseña por defecto.
    /// </summary>
    Task EnsureSeedAsync(CancellationToken cancellationToken = default);
}
